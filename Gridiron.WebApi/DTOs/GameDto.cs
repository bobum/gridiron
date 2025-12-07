namespace Gridiron.WebApi.DTOs;

/// <summary>
/// Game response DTO with summary information.
/// </summary>
public class GameDto
{
    public int Id { get; set; }

    public int HomeTeamId { get; set; }

    public int AwayTeamId { get; set; }

    public string HomeTeamName { get; set; } = string.Empty;

    public string AwayTeamName { get; set; } = string.Empty;

    public int HomeScore { get; set; }

    public int AwayScore { get; set; }

    public int? RandomSeed { get; set; }

    public bool IsComplete { get; set; }
}


/// <summary>
/// Detailed game response with full play-by-play.
/// </summary>
public class GameDetailDto : GameDto
{
    public List<PlayDto> Plays { get; set; } = new ();

    public GameStatsDto Stats { get; set; } = new ();
}

/// <summary>
/// Game statistics summary.
/// </summary>
public class GameStatsDto
{
    public int TotalYards { get; set; }

    public int PassingYards { get; set; }

    public int RushingYards { get; set; }

    public int Turnovers { get; set; }

    public int Penalties { get; set; }

    public int TimeOfPossession { get; set; }
}
