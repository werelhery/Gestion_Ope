using GestionOpe.Api.Middlewares;
using GestionOpe.Application;
using GestionOpe.Application.Interfaces;
using GestionOpe.Infrastructure;
using GestionOpe.Infrastructure.Persistence;
using GestionOpe.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Add API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestion_Ope API - SaaS de Gestion Opérationnelle & Logistique",
        Version = "v1",
        Description = "API REST .NET 8 en Clean Architecture avec CQRS (MediatR), FluentValidation, EF Core et JWT."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "En-tête d'autorisation JWT utilisant le schéma Bearer. Exemple: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
});

// 4. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:80", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 5. Database Migration & Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        await AppDbContextSeeder.SeedAsync(db, passwordHasher, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données.");
    }
}

// 6. HTTP Pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment() || true) // Enable swagger for review
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GestionOpe API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow, service = "GestionOpe.Api" }));

app.Run();

// For Integration Testing
public partial class Program { }
