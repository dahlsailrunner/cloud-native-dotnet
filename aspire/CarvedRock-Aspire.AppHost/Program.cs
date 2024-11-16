using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var apmAuthHeader = builder.Configuration.GetValue<string>("ApmServerAuth")!; // user secrets

var carvedrockdb = builder.AddPostgres("postgres")
                          .AddDatabase("CarvedRockPostgres");

var emailService = builder.AddSmtp4Dev("smtp-4-dev")  // custom extension
    .WithExternalHttpEndpoints();

var identityserver = builder.AddProject<Projects.CarvedRock_Identity>("carvedrock-identity")
    //.WithOtherOpenTelemetryService(apmAuthHeader)
    .WithSharedLoggingLevels()
    .WithExternalHttpEndpoints();

var identityEndpoint = identityserver.GetEndpoint("https");

var api = builder.AddProject<Projects.CarvedRock_Api>("carvedrock-api")
    .WithReference(carvedrockdb)
    .WithSharedLoggingLevels()
    //.WithOtherOpenTelemetryService(apmAuthHeader)
    .WithEnvironment("Auth__Authority", identityEndpoint);

builder.AddProject<Projects.CarvedRock_WebApp>("carvedrock-webapp")
    .WithReference(api)
    .WithSharedLoggingLevels()
    //.WithOtherOpenTelemetryService(apmAuthHeader)
    .WithEnvironment("Auth__Authority", identityEndpoint)
    .WithReference(emailService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
