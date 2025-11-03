using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
// using Swashbuckle.AspNetCore.Annotations; // opcional, desnecessário se não usar [SwaggerOperation]
using FluentValidation;
using FluentValidation.AspNetCore;
using OrganizedScannApi.Api.Middlewares;
using OrganizedScannApi.Infrastructure;
using OrganizedScannAPI.Application.Pagination; // <- cuidado com o namespace (I maiúsculo)
using OrganizedScannApi.Application.UseCases.Motorcycles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using OrganizedScannApi.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Versionamento da API
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrganizedScann API",
        Version = "v1",
        Description = "API RESTful com 3 entidades (Motorcycles, Portals, Users), paginação, HATEOAS, Health Checks, JWT e ML.NET",
        Contact = new OpenApiContact
        {
            Name = "OrganizedScann Team",
            Email = "support@organizedscann.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // JWT Bearer Configuration
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 🔑 Faz o Swagger/UI usar a MESMA origem da página, evitando CORS/mixed content.
    c.AddServer(new OpenApiServer { Url = "/" });

    // Exemplos
    c.ExampleFilters();
});

// Swagger example providers
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.MotorcycleExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.PortalExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.UserExample>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PaginatedRequest>();

// Infrastructure (DbContext - Oracle via AddInfrastructure)
builder.Services.AddInfrastructure(builder.Configuration);

// Application Services
builder.Services.AddScoped<MotorcycleService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "api" })
    .AddCheck("memory", () => 
    {
        var allocated = GC.GetTotalMemory(false);
        var data = new Dictionary<string, object>();
        data.Add("allocated", allocated);
        var status = allocated < 1_000_000_000 ? HealthStatus.Healthy : HealthStatus.Degraded;
        return HealthCheckResult.Healthy(data: data);
    }, tags: new[] { "memory" });

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

// CORS (liberado para testes; em prod, prefira WithOrigins(...))
builder.Services.AddCors(o =>
    o.AddPolicy("Default", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
);

var app = builder.Build();

// HTTPS primeiro (evita mixed content quando você abre por https)
app.UseHttpsRedirection();

// CORS
app.UseCors("Default");

// Swagger (endpoint RELATIVO — não use http://localhost:xxxx aqui)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrganizedScann API v1");
    c.RoutePrefix = "swagger";
});

// Middlewares
app.UseMiddleware<ErrorHandlingMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

// Make Program visible for integration tests
public partial class Program { }
