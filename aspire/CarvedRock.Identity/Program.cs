using CarvedRock.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        //.WriteTo.Seq("http://localhost:5341")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!;
            options.ResourceAttributes.Add("service.name", builder.Configuration["OTEL_SERVICE_NAME"]!);
            var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
            foreach (var header in headers)
            {
                var (key, value) = header.Split('=') switch
                {
                [string k, string v] => (k, v),
                    var v => throw new Exception($"Invalid header format {v}")
                };

                options.Headers.Add(key, value);
            }
        })); 

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