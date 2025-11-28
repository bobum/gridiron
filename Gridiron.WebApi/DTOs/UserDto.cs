namespace Gridiron.WebApi.DTOs;

/// <summary>
/// User response DTO
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public List<UserLeagueRoleDto> LeagueRoles { get; set; } = new();
}

/// <summary>
/// User league role DTO
/// </summary>
public class UserLeagueRoleDto
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public DateTime AssignedAt { get; set; }
}

/// <summary>
/// Request to assign a user to a league role
/// </summary>
public class AssignLeagueRoleRequest
{
    public int LeagueId { get; set; }
    public string Role { get; set; } = string.Empty; // "Commissioner" or "GeneralManager"
    public int? TeamId { get; set; } // Required if Role is GeneralManager
}
