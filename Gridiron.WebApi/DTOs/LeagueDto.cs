namespace Gridiron.WebApi.DTOs;

/// <summary>
/// League response DTO.
/// </summary>
public class LeagueDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Season { get; set; }

    public bool IsActive { get; set; }

    public int TotalTeams { get; set; }

    public int TotalConferences { get; set; }
}

/// <summary>
/// Detailed league response with full structure.
/// </summary>
public class LeagueDetailDto : LeagueDto
{
    public List<ConferenceDto> Conferences { get; set; } = new ();
}

/// <summary>
/// Conference response DTO.
/// </summary>
public class ConferenceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<DivisionDto> Divisions { get; set; } = new ();
}

/// <summary>
/// Division response DTO.
/// </summary>
public class DivisionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TeamDto> Teams { get; set; } = new ();
}

/// <summary>
/// Request to create a new league.
/// </summary>
public class CreateLeagueRequest
{
    public string Name { get; set; } = string.Empty;

    public int NumberOfConferences { get; set; }

    public int DivisionsPerConference { get; set; }

    public int TeamsPerDivision { get; set; }
}
