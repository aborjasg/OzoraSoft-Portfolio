var builder = DistributedApplication.CreateBuilder(args);

var apiUtils = builder.AddProject<Projects.OzoraSoft_API_Utils>("ozorasoft-api-utils");
//.WithHttpHealthCheck("/health");

var apiServices = builder.AddProject<Projects.OzoraSoft_API_Services>("ozorasoft-api-services");
//.WithHttpHealthCheck("/health");

builder.AddProject<Projects.OzoraSoft_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiUtils)
    .WithReference(apiServices)
    .WaitFor(apiServices);

builder.Build().Run();
