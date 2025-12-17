var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Stargate_API>("stargate-api");

var ui = builder.AddProject<Projects.Stargate_UI_Server>("stargate-ui-server")
    .WithReference(api);

builder.Build().Run();
