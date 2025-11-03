using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Annotations;
using FluentValidation;
using FluentValidation.AspNetCore;
using OrganizedScannApi.Api.Middlewares;
using OrganizedScannApi.Infrastructure;
using OrganizedScannAPI.Application.Pagination; // <- I maiúsculo
using OrganizedScannApi.Application.UseCases.Motorcycles;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrganizedScann API",
        Version = "v1",
        Description = "API RESTful com 3 entidades (Motorcycles, Portals, Users), paginação e HATEOAS."
    });
    // c.EnableAnnotations(); // <- REMOVIDO para não exigir o pacote Annotations
    c.ExampleFilters();
});

// Swagger example providers
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.MotorcycleExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.PortalExample>();
builder.Services.AddSwaggerExamplesFromAssemblyOf<OrganizedScannApi.Api.Swagger.UserExample>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<PaginatedRequest>(); // precisa do 'using FluentValidation'

// Infrastructure (DbContext - Oracle via AddInfrastructure)
builder.Services.AddInfrastructure(builder.Configuration);

// Application Services
builder.Services.AddScoped<MotorcycleService>();

// CORS policy explícita
builder.Services.AddCors(o => o.AddPolicy("Default", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrganizedScann API v1");
    c.RoutePrefix = "swagger";
});

// Pipeline
app.UseHttpsRedirection();
app.UseCors("Default");
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
