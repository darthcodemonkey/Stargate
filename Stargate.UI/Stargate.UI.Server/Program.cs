using Microsoft.Extensions.FileProviders;

// Helper method to resolve Angular client build output path
static string GetAngularClientDistPath(string? configuredPath, IWebHostEnvironment environment)
{
    if (!string.IsNullOrWhiteSpace(configuredPath) && Directory.Exists(configuredPath))
    {
        return configuredPath;
    }

    // Try multiple approaches to find the path
    var possiblePaths = new List<string>();
    
    // Approach 1: From ContentRootPath (most reliable for Aspire)
    // When running from Aspire, ContentRootPath is usually the project directory
    var pathFromContentRoot = Path.GetFullPath(Path.Combine(
        environment.ContentRootPath,
        "..", "stargate.ui.client", "dist", "stargate.ui.client", "browser"
    ));
    possiblePaths.Add(pathFromContentRoot);
    
    // Also try without going up a level (in case ContentRootPath is already at UI folder level)
    var pathFromContentRootDirect = Path.GetFullPath(Path.Combine(
        environment.ContentRootPath,
        "..", "..", "Stargate.UI", "stargate.ui.client", "dist", "stargate.ui.client", "browser"
    ));
    possiblePaths.Add(pathFromContentRootDirect);
    
    // Approach 2: From assembly location
    var assemblyLocation = typeof(Program).Assembly.Location;
    if (!string.IsNullOrEmpty(assemblyLocation))
    {
        var binDirectory = Path.GetDirectoryName(assemblyLocation)!;
        var serverProjectRoot = Path.GetFullPath(Path.Combine(binDirectory, "..", "..", ".."));
        var pathFromAssembly = Path.GetFullPath(Path.Combine(
            serverProjectRoot,
            "stargate.ui.client",
            "dist",
            "stargate.ui.client",
            "browser"
        ));
        possiblePaths.Add(pathFromAssembly);
    }
    
    // Approach 3: Relative to current working directory
    var currentDir = Directory.GetCurrentDirectory();
    var pathFromCurrentDir = Path.GetFullPath(Path.Combine(
        currentDir,
        "stargate.ui.client", "dist", "stargate.ui.client", "browser"
    ));
    possiblePaths.Add(pathFromCurrentDir);
    
    // Return the first path that exists, or the first one if none exist (for logging)
    foreach (var path in possiblePaths)
    {
        if (Directory.Exists(path) && File.Exists(Path.Combine(path, "index.html")))
        {
            return path;
        }
    }
    
    // Return the most likely path even if it doesn't exist (for error logging)
    return possiblePaths[0];
}

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for proxying API calls to stargate-api service
// Aspire will automatically configure this when using WithReference in AppHost
builder.Services.AddHttpClient("stargate-api");

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure static files - serve Angular app from build output
var clientDistPath = GetAngularClientDistPath(
    builder.Configuration["AngularClientDistPath"],
    app.Environment
);

app.Logger.LogInformation("=== Static Files Configuration ===");
app.Logger.LogInformation("ContentRootPath: {ContentRoot}", app.Environment.ContentRootPath);
app.Logger.LogInformation("WebRootPath: {WebRoot}", app.Environment.WebRootPath);
app.Logger.LogInformation("Looking for Angular build output at: {Path}", clientDistPath);
app.Logger.LogInformation("Directory exists: {Exists}", Directory.Exists(clientDistPath));

if (Directory.Exists(clientDistPath))
{
    var indexPath = Path.Combine(clientDistPath, "index.html");
    app.Logger.LogInformation("index.html exists: {Exists} at {Path}", File.Exists(indexPath), indexPath);
    
    if (File.Exists(indexPath))
    {
        // Serve static files from Angular build output
        // IMPORTANT: UseDefaultFiles must come BEFORE UseStaticFiles
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(clientDistPath),
            RequestPath = ""
        });
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(clientDistPath),
            RequestPath = ""
        });
        
        app.Logger.LogInformation("✓ Serving Angular app from: {Path}", clientDistPath);
    }
}
else
{
    app.Logger.LogWarning("✗ Angular build output not found at: {Path}", clientDistPath);
    // Fallback to default wwwroot if it exists
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stargate UI API");
        c.RoutePrefix = "swagger"; // Swagger UI available at /swagger instead of root
    });
}

app.UseHttpsRedirection();

// Proxy /api/* requests to the stargate-api service
// This MUST be before UseRouting() to intercept API calls
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/api"))
    {
        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        
        // Try multiple ways to get the API service URL from Aspire service discovery
        string? apiBaseUrl = null;
        
        // Method 1: Try Aspire's HttpClient (if BaseAddress is set)
        try
        {
            var aspireClient = httpClientFactory.CreateClient("stargate-api");
            apiBaseUrl = aspireClient.BaseAddress?.ToString()?.TrimEnd('/');
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not get HttpClient for stargate-api");
        }
        
        // Method 2: Try configuration keys (Aspire injects service URLs here)
        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            apiBaseUrl = configuration["services__stargate-api__https__0"] 
                ?? configuration["services__stargate-api__http__0"]
                ?? configuration["ConnectionStrings__stargate-api"]
                ?? configuration["ApiBaseUrl"];
        }
        
        // Method 3: Fallback (should not be needed in Aspire)
        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            apiBaseUrl = "https://localhost:7204"; // Default API port
            logger.LogWarning("Using fallback API URL: {ApiBaseUrl}. Service discovery may not be working.", apiBaseUrl);
        }
        
        logger.LogInformation("API Proxy intercepted: {Method} {Path} -> {BaseUrl}", context.Request.Method, path, apiBaseUrl);
        
        var httpClient = httpClientFactory.CreateClient("stargate-api");
        var requestPath = path.Value!;
        var queryString = context.Request.QueryString.Value ?? "";
        var apiUrl = $"{apiBaseUrl}{requestPath}{queryString}";
        
        logger.LogInformation("Proxying {Method} {RequestPath} to {ApiUrl}", context.Request.Method, requestPath, apiUrl);
        
        try
        {
            var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), apiUrl);
            
            // Copy request headers (excluding Host)
            foreach (var header in context.Request.Headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) &&
                    !header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                    {
                        requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }
            }
            
            // Copy request body for POST/PUT/PATCH (always try to read body for these methods)
            if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                
                // Read body even if ContentLength is not set
                var bodyStream = new MemoryStream();
                await context.Request.Body.CopyToAsync(bodyStream);
                bodyStream.Position = 0;
                
                // Only add content if body has data
                if (bodyStream.Length > 0)
                {
                    requestMessage.Content = new StreamContent(bodyStream);
                    
                    // Set content type
                    if (context.Request.ContentType != null && !string.IsNullOrWhiteSpace(context.Request.ContentType))
                    {
                        requestMessage.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
                    }
                    else
                    {
                        requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    }
                }
            }
            
            var response = await httpClient.SendAsync(requestMessage);
            
            // Copy response status and headers
            context.Response.StatusCode = (int)response.StatusCode;
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            foreach (var header in response.Content.Headers)
            {
                if (!header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }
            }
            
            await response.Content.CopyToAsync(context.Response.Body);
            logger.LogInformation("Proxy response: {StatusCode}", response.StatusCode);
            return;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error proxying request to API: {ApiUrl}", apiUrl);
            context.Response.StatusCode = 502;
            await context.Response.WriteAsync($"Bad Gateway: Failed to proxy to API at {apiBaseUrl}");
            return;
        }
    }
    
    await next();
});

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for Angular routing (SPA routing support)
// This MUST be last, after all other route mappings
if (Directory.Exists(clientDistPath) && File.Exists(Path.Combine(clientDistPath, "index.html")))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(clientDistPath)
    });
    app.Logger.LogInformation("✓ Configured SPA fallback to index.html");
}
else
{
    app.MapFallbackToFile("/index.html");
}

app.Run();
