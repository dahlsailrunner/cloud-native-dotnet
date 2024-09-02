using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using CarvedRock.WebApp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // serilog configured in here
        
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

builder.AddMailKitClient("SmtpUri"); // Added with MailKit
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
