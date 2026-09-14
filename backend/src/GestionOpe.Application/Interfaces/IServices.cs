using GestionOpe.Domain.Entities;

namespace GestionOpe.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    int ExpirationMinutes { get; }
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
