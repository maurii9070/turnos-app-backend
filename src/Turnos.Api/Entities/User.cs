using Turnos.Api.Entities.Enums;

namespace Turnos.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Dni { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public Guid? RegisteredBy { get; set; }
    public bool MustChangePassword { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
    public User? RegisteredByUser { get; set; }
}
