namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Player response DTO
/// </summary>
public class PlayerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Position { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Height { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int Age { get; set; }
    public int Exp { get; set; }
    public string College { get; set; } = string.Empty;
    public int? TeamId { get; set; }

    // Attributes
    public int Speed { get; set; }
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Awareness { get; set; }
    public int Morale { get; set; }
    public int Discipline { get; set; }

    // Position-specific skills
    public int Passing { get; set; }
    public int Catching { get; set; }
    public int Rushing { get; set; }
    public int Blocking { get; set; }
    public int Tackling { get; set; }
    public int Coverage { get; set; }
    public int Kicking { get; set; }

    // Status
    public int Health { get; set; }
    public bool IsInjured { get; set; }
}

/// <summary>
/// Detailed player response with stats
/// </summary>
public class PlayerDetailDto : PlayerDto
{
    public Dictionary<string, int> GameStats { get; set; } = new();
    public Dictionary<string, int> SeasonStats { get; set; } = new();
    public Dictionary<string, int> CareerStats { get; set; } = new();
}
