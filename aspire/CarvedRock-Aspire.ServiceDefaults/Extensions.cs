using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // examples of some resilience customization
            //http.AddStandardResilienceHandler(opts =>
            //{
            //    opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
            //    opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
            //    opts.Retry.MaxRetryAttempts = 7;
            //    opts.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            //});
            //http.AddStandardHedgingHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static readonly string[] _excludeFromTraces = [
        "aspnetcore-browser-refresh.js",
        "browserLink",
        //"health",
       ];
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        (builder as WebApplicationBuilder)?.AddCustomSerilog();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = (httpContext) =>
                    {
                        var excludeFromTraces = _excludeFromTraces;
                        if (excludeFromTraces.Any(ex => httpContext.Request.Path.Value?.Contains(ex) ?? false))
                        {
                            return false;
                        }
                        return true;
                    };
                })
                // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                //.AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation(options =>
                {
                    options.FilterHttpRequestMessage = (request) =>
                    {
                        return !request.RequestUri?.ToString().Contains("getScriptTag",
                            StringComparison.InvariantCultureIgnoreCase) ?? true;
                    };
                });
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddRequestTimeouts();
        builder.Services.AddOutputCache();

        builder.Services.AddRequestTimeouts(
        configure: static timeouts =>
            timeouts.AddPolicy("HealthChecks", TimeSpan.FromSeconds(5)));

        builder.Services.AddOutputCache(
            configureOptions: static caching =>
                caching.AddPolicy("HealthChecks",
                build: static policy => policy.Expire(TimeSpan.FromSeconds(10))));

        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    private static IHostBuilder AddCustomSerilog(this WebApplicationBuilder builder)
    {
        return builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .WriteTo.Console()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .Enrich.With<ActivityEnricher>();

            var otlpExporterEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (string.IsNullOrEmpty(otlpExporterEndpoint)) return;

            loggerConfig
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpExporterEndpoint;
                    options.ResourceAttributes.Add("service.name", builder.Configuration["OTEL_SERVICE_NAME"]!);
                    options.ResourceAttributes.Add("service.instance.id", Environment.MachineName);
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
                });
        });
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {


        var healthChecks = app.MapGroup("");

        healthChecks
            .CacheOutput("HealthChecks")
            .WithRequestTimeout("HealthChecks");

        // All health checks must pass for app to be
        // considered ready to accept traffic after starting
        healthChecks.MapHealthChecks("/health");

        // Only health checks tagged with the "live" tag
        // must pass for app to be considered alive
        healthChecks.MapHealthChecks("/alive", new()
        {
            Predicate = static r => r.Tags.Contains("live")
        });

        app.UseRequestTimeouts();
        app.UseOutputCache();

        return app;
    }
}
