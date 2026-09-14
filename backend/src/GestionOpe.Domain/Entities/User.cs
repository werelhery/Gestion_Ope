using GestionOpe.Domain.Common;
using GestionOpe.Domain.Enums;

namespace GestionOpe.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public UserRole Role { get; set; } = UserRole.LogisticsManager;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }
}
