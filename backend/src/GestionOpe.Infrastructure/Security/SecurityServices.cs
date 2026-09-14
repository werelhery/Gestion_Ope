using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GestionOpe.Application.Interfaces;
using GestionOpe.Domain.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GestionOpe.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int ExpirationMinutes => int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 480;

    public string GenerateToken(User user)
    {
        var secretKey = _configuration["Jwt:Secret"] ?? "SuperSecretKeyGestionOpe2026SecureKeyWithAtLeast32Chars!";
        var issuer = _configuration["Jwt:Issuer"] ?? "GestionOpe";
        var audience = _configuration["Jwt:Audience"] ?? "GestionOpeClient";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return $"{Convert.ToBase64String(salt)}.{hashed}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var originalHash = parts[1];

        string incomingHashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return originalHash == incomingHashed;
    }
}
