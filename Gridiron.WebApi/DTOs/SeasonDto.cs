using DomainObjects;

namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Season response DTO.
/// </summary>
public class SeasonDto
{
    public int Id { get; set; }

    public int LeagueId { get; set; }

    public int Year { get; set; }

    public int CurrentWeek { get; set; }

    public string Phase { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int RegularSeasonWeeks { get; set; }

    public int? ChampionTeamId { get; set; }

    public int WeekCount { get; set; }

    public int TotalGames { get; set; }
}

/// <summary>
/// Detailed season response with weeks.
/// </summary>
public class SeasonDetailDto : SeasonDto
{
    public List<SeasonWeekDto> Weeks { get; set; } = new ();
}

/// <summary>
/// Season week response DTO.
/// </summary>
public class SeasonWeekDto
{
    public int Id { get; set; }

    public int SeasonId { get; set; }

    public int WeekNumber { get; set; }

    public string Phase { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public bool IsComplete { get; set; }

    public int GameCount { get; set; }

    public List<ScheduledGameDto> Games { get; set; } = new ();
}

/// <summary>
/// Scheduled game response DTO (used in schedule view).
/// </summary>
public class ScheduledGameDto
{
    public int Id { get; set; }

    public int HomeTeamId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string? HomeTeamCity { get; set; }

    public int AwayTeamId { get; set; }

    public string AwayTeamName { get; set; } = string.Empty;

    public string? AwayTeamCity { get; set; }

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public bool IsComplete { get; set; }

    public DateTime? PlayedAt { get; set; }
}

/// <summary>
/// Request to create a new season.
/// </summary>
public class CreateSeasonRequest
{
    public int Year { get; set; }

    public int? RegularSeasonWeeks { get; set; }
}

/// <summary>
/// Request to generate a schedule for a season.
/// </summary>
public class GenerateScheduleRequest
{
    /// <summary>
    /// Gets or sets optional seed for reproducible schedule generation.
    /// </summary>
    public int? Seed { get; set; }
}

/// <summary>
/// Response from schedule generation.
/// </summary>
public class GenerateScheduleResponse
{
    public int SeasonId { get; set; }

    public int TotalWeeks { get; set; }

    public int TotalGames { get; set; }

    public int GamesPerTeam { get; set; }

    public List<string> Warnings { get; set; } = new ();

    public SeasonDetailDto Season { get; set; } = null!;
}
