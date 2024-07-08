using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Exceptions;
using Serilog.Enrichers.Span;
using CarvedRock.WebApp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;

public partial class Program {
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console()
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .Enrich.With<ActivityEnricher>()
            .WriteTo.Seq("http://localhost:5341")
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
             });
        });

        var authority = builder.Configuration.GetValue<string>("Auth:Authority"); 

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies", options => options.AccessDeniedPath = "/AccessDenied")
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = authority;  // not properly resolved without setting the environment variable in apphost
            options.ClientId = "carvedrock-webapp";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("role");
            options.Scope.Add("carvedrockapi");
            options.Scope.Add("offline_access");
            options.GetClaimsFromUserInfoEndpoint = true;
            options.ClaimActions.MapJsonKey("role", "role", "role");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = "email",
                RoleClaimType = "role"
            };
            options.SaveTokens = true;
        });
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient<IProductService, ProductService>();
        builder.Services.AddScoped<IEmailSender, EmailService>();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.UseExceptionHandler("/Error");

        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages().RequireAuthorization();

        app.Run();
    }
} 