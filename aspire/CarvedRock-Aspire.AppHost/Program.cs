using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var apmAuthHeader = builder.Configuration.GetValue<string>("ApmServerAuth")!; // user secrets

var carvedrockdb = builder.AddPostgres("postgres")
                          .AddDatabase("CarvedRockPostgres");

var emailService = builder.AddSmtp4Dev("SmtpUri");  // custom extension

var identityserver = builder.AddProject<Projects.CarvedRock_Identity>("carvedrock-identity")
    //.WithOtherOpenTelemetryService(apmAuthHeader)
    .WithSharedLoggingLevels();

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
    .WithReference(emailService);    

builder.Build().Run();
