namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Request DTO for updating a league
/// All properties are optional - only provided values will be updated
/// </summary>
public class UpdateLeagueRequest
{
    /// <summary>
    /// New name for the league
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New season for the league (must be between 1900 and current year + 5)
    /// </summary>
    public int? Season { get; set; }

    /// <summary>
    /// New active status for the league
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request DTO for updating a conference
/// All properties are optional - only provided values will be updated
/// </summary>
public class UpdateConferenceRequest
{
    /// <summary>
    /// New name for the conference
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request DTO for updating a division
/// All properties are optional - only provided values will be updated
/// </summary>
public class UpdateDivisionRequest
{
    /// <summary>
    /// New name for the division
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Request DTO for updating a team
/// All properties are optional - only provided values will be updated
/// </summary>
public class UpdateTeamRequest
{
    /// <summary>
    /// New name for the team
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New city for the team
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// New budget for the team
    /// </summary>
    public int? Budget { get; set; }

    /// <summary>
    /// New championships count
    /// </summary>
    public int? Championships { get; set; }

    /// <summary>
    /// New wins count
    /// </summary>
    public int? Wins { get; set; }

    /// <summary>
    /// New losses count
    /// </summary>
    public int? Losses { get; set; }

    /// <summary>
    /// New ties count
    /// </summary>
    public int? Ties { get; set; }

    /// <summary>
    /// New fan support (0-100)
    /// </summary>
    public int? FanSupport { get; set; }

    /// <summary>
    /// New chemistry (0-100)
    /// </summary>
    public int? Chemistry { get; set; }
}
