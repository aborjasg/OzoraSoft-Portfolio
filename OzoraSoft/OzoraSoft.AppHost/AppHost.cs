using System.Net;

var builder = DistributedApplication.CreateBuilder(args);

// IdentityServer project
var identity = builder.AddProject<Projects.OzoraSoft_IdentityServer>("ozorasoft-identity")
.WithHttpHealthCheck("/health");

// API project for Main Services
var apiServices = builder.AddProject<Projects.OzoraSoft_API_Services>("ozorasoft-api-services")
.WithHttpHealthCheck("/health");

// API project for Utils features
var apiUtils = builder.AddProject<Projects.OzoraSoft_API_Utils>("ozorasoft-api-utils")
.WithHttpHealthCheck("/health");

builder.AddProject<Projects.OzoraSoft_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    //.WithReference(identity)    
    .WithReference(apiServices)
    .WithReference(apiUtils);
    //.WaitFor(identity);

builder.Build().Run();
