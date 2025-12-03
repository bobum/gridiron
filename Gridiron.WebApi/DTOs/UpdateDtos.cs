namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Request DTO for updating a league
/// All properties are optional - only provided values will be updated.
/// </summary>
public class UpdateLeagueRequest
{
    /// <summary>
    /// Gets or sets new name for the league.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets new season for the league (must be between 1900 and current year + 5).
    /// </summary>
    public int? Season { get; set; }

    /// <summary>
    /// Gets or sets new active status for the league.
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request DTO for updating a conference
/// All properties are optional - only provided values will be updated.
/// </summary>
public class UpdateConferenceRequest
{
    /// <summary>
    /// Gets or sets new name for the conference.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request DTO for updating a division
/// All properties are optional - only provided values will be updated.
/// </summary>
public class UpdateDivisionRequest
{
    /// <summary>
    /// Gets or sets new name for the division.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request DTO for updating a team
/// All properties are optional - only provided values will be updated.
/// </summary>
public class UpdateTeamRequest
{
    /// <summary>
    /// Gets or sets new name for the team.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets new city for the team.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Gets or sets new budget for the team.
    /// </summary>
    public int? Budget { get; set; }

    /// <summary>
    /// Gets or sets new championships count.
    /// </summary>
    public int? Championships { get; set; }

    /// <summary>
    /// Gets or sets new wins count.
    /// </summary>
    public int? Wins { get; set; }

    /// <summary>
    /// Gets or sets new losses count.
    /// </summary>
    public int? Losses { get; set; }

    /// <summary>
    /// Gets or sets new ties count.
    /// </summary>
    public int? Ties { get; set; }

    /// <summary>
    /// Gets or sets new fan support (0-100).
    /// </summary>
    public int? FanSupport { get; set; }

    /// <summary>
    /// Gets or sets new chemistry (0-100).
    /// </summary>
    public int? Chemistry { get; set; }
}
