using System.IdentityModel.Tokens.Jwt;
using CarvedRock.Data;
using CarvedRock.Domain;
using CarvedRock.Api;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using CarvedRock.Domain.Mapping;
using FluentValidation;
using CarvedRock.Core;

var builder = WebApplication.CreateBuilder(args);        
builder.AddServiceDefaults(); // serilog configured inside here

builder.Services.AddProblemDetails(opts => // built-in problem details support
    opts.CustomizeProblemDetails = (ctx) =>
    {
        if (!ctx.ProblemDetails.Extensions.ContainsKey("traceId"))
        {
            string? traceId = Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
            ctx.ProblemDetails.Extensions.Add(new KeyValuePair<string, object?>("traceId", traceId));
        }
        var exception = ctx.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ctx.ProblemDetails.Status == 500)
        {
            ctx.ProblemDetails.Detail = "An error occurred in our API. Use the trace id when contacting us.";
        }
    }
);

var authority = builder.Configuration.GetValue<string>("Auth:Authority");
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "email",
            RoleClaimType = "role",
            ValidateAudience = false
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductLogic, ProductLogic>();

// Postgres -----------------------------
//builder.Services.AddDbContext<LocalContext>(options => options
//    .UseNpgsql(builder.Configuration.GetConnectionString("CarvedRockPostgres"))
//    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.AddNpgsqlDbContext<LocalContext>("CarvedRockPostgres");

// SQL Server ---------------------------
//builder.Services.AddDbContext<LocalContext>(options => options
//    .UseSqlServer(builder.Configuration.GetConnectionString("CarvedRockSqlServer"))
//    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

builder.Services.AddScoped<ICarvedRockRepository, CarvedRockRepository>();

builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<NewProductValidator>();

var app = builder.Build();
//app.UseSerilogRequestLogging(options =>
//{
//    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
//    {
//        diagnosticContext.Set("client_id", httpContext.User.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value);
//    };
//});
app.MapDefaultEndpoints();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    SetupDevelopment(app);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserScopeMiddleware>();
app.MapControllers().RequireAuthorization();

app.Run();

static void SetupDevelopment(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<LocalContext>();
        context.MigrateAndCreateData();
    }

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("carvedrock-swaggerui");
        options.OAuthAppName("CarvedRock API");
        options.OAuthUsePkce();
    });
}
