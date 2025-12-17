using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Data;
using StargateAPI.Domain.Data;
using StargateAPI.Domain.Interfaces;
using StargateAPI.Domain.Repositories;
using StargateAPI.Services.Services;

// Configure Serilog with database logging
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("StargateDatabase") 
    ?? throw new InvalidOperationException("Connection string 'StargateDatabase' is required");

var columnOptions = new ColumnOptions();
columnOptions.Store.Remove(StandardColumn.MessageTemplate);
columnOptions.Store.Remove(StandardColumn.Properties);
columnOptions.Store.Add(StandardColumn.LogEvent);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            SchemaName = "dbo",
            AutoCreateSqlTable = true
        },
        columnOptions: columnOptions)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting Stargate API application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    
    // Add CORS to allow UI.Server to call the API
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configure SQL Server Database
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Error("Connection string 'StargateDatabase' is missing from configuration");
        throw new InvalidOperationException("Connection string 'StargateDatabase' is required");
    }

    builder.Services.AddDbContext<StargateContext>(options =>
    {
        options.UseSqlServer(connectionString);
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    Log.Information("Database connection configured: {ConnectionString}", 
        connectionString.Replace("Trusted_Connection=true;", "[Trusted Connection]"));

    // Register Repositories
    builder.Services.AddScoped<IPersonRepository, PersonRepository>();
    builder.Services.AddScoped<IAstronautDutyRepository, AstronautDutyRepository>();
    builder.Services.AddScoped<IAstronautDetailRepository, AstronautDetailRepository>();

    // Register Services
    builder.Services.AddScoped<IPersonService, PersonService>();
    builder.Services.AddScoped<IAstronautDutyService, AstronautDutyService>();

    Log.Information("Services registered successfully");

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("Swagger UI enabled for Development environment");
    }

    // Apply pending migrations and seed database (skip for in-memory databases used in tests)
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            if (context.Database.IsSqlServer())
            {
                // Check if database can be connected to
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    Log.Warning("Cannot connect to database. Skipping migrations and seeding.");
                }
                else
                {
                    // Get pending migrations
                    var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    
                    if (pendingMigrations.Any())
                    {
                        try
                        {
                            Log.Information("Applying {Count} pending migration(s): {Migrations}", 
                                pendingMigrations.Count, string.Join(", ", pendingMigrations));
                            await context.Database.MigrateAsync();
                            Log.Information("Database migrations applied successfully");
                        }
                        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("already an object named"))
                        {
                            Log.Warning("Migration failed because tables already exist. " +
                                      "This usually means migration history is out of sync. " +
                                      "Tables appear to already exist in the database. " +
                                      "If you need to sync migration history, you can manually insert records into __EFMigrationsHistory. " +
                                      "Continuing with seeding...");
                        }
                    }
                    else
                    {
                        Log.Information("Database is up to date - no pending migrations");
                    }

                    // Seed database with test data
                    await DatabaseSeeder.SeedAsync(context, logger);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error applying database migrations or seeding: {Message}", ex.Message);
            // Don't throw - allow application to start even if migrations/seeding fail
            // This allows the app to start and show proper error messages to users
        }
    }

    app.UseHttpsRedirection();
    
    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Stargate API application started successfully");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program class for testing
public partial class Program { }
