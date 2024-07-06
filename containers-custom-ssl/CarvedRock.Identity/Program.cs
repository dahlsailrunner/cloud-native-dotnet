using CarvedRock.Identity;
using Serilog;
using System.Net;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var useCustomLocalCert = builder.Configuration.GetValue<bool>("UseCustomLocalCert");
    if (useCustomLocalCert)
    {
        builder.WebHost.ConfigureKestrel((ctx, options) =>
        {
            options.Listen(IPAddress.Any, 8099, listenOptions =>
            {
                listenOptions.UseHttps("keys/cr-id-local.pfx", "Learning1sGreat!");
            });
        });
    }

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .WriteTo.Seq(ctx.Configuration.GetValue<string>("SeqAddress")!)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}