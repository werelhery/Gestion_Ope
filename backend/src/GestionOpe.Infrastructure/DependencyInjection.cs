using System.Text;
using GestionOpe.Application.Interfaces;
using GestionOpe.Domain.Interfaces;
using GestionOpe.Infrastructure.Persistence;
using GestionOpe.Infrastructure.Persistence.Repositories;
using GestionOpe.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GestionOpe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString) && (connectionString.Contains("Host=") || connectionString.Contains("Server=")))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                var sqliteConnection = string.IsNullOrEmpty(connectionString)
                    ? "Data Source=gestion_ope.db"
                    : connectionString;
                options.UseSqlite(sqliteConnection);
            }
        });

        // Repositories
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ICarrierRepository, CarrierRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Security
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // JWT Authentication Configuration
        var secretKey = configuration["Jwt:Secret"] ?? "SuperSecretKeyGestionOpe2026SecureKeyWithAtLeast32Chars!";
        var issuer = configuration["Jwt:Issuer"] ?? "GestionOpe";
        var audience = configuration["Jwt:Audience"] ?? "GestionOpeClient";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
