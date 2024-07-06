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
        builder.Logging.ClearProviders();

        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console()
            .Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            .Enrich.With<ActivityEnricher>()
            .WriteTo.Seq("http://localhost:5341");
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
            options.Authority = authority;
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
        builder.Services.AddHealthChecks();

        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient<IProductService, ProductService>();
        builder.Services.AddScoped<IEmailSender, EmailService>();

        var app = builder.Build();

        app.UseExceptionHandler("/Error");

        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages().RequireAuthorization();
        app.MapHealthChecks("health").AllowAnonymous();

        app.Run();
    }
} 