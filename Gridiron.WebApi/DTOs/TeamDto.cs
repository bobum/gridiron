namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Team response DTO.
/// </summary>
public class TeamDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public int Budget { get; set; }

    public int Championships { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Ties { get; set; }

    public int FanSupport { get; set; }

    public int Chemistry { get; set; }
}

/// <summary>
/// Detailed team response with roster.
/// </summary>
public class TeamDetailDto : TeamDto
{
    public List<PlayerDto> Roster { get; set; } = new ();

    public CoachDto? HeadCoach { get; set; }
}

/// <summary>
/// Coach information.
/// </summary>
public class CoachDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
