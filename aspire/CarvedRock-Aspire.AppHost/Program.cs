var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var carvedrockdb = postgres.AddDatabase("CarvedRockPostgres");

var emailService = builder.AddSmtp4Dev("SmtpUri");

var identityserver = builder.AddProject<Projects.CarvedRock_Identity>("carvedrock-identity");
var identityEndpoint = identityserver.GetEndpoint("https");

var api = builder.AddProject<Projects.CarvedRock_Api>("carvedrock-api")
    .WithReference(carvedrockdb)
    .WithEnvironment("Auth__Authority", identityEndpoint);

builder.AddProject<Projects.CarvedRock_WebApp>("carvedrock-webapp")
    .WithReference(api)    
    .WithEnvironment("Auth__Authority", identityEndpoint)
    .WithReference(emailService);    

builder.Build().Run();
