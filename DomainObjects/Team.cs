using DomainObjects;
using static DomainObjects.StatTypes;

public class Team
{
    public int Id { get; set; }  // Primary key for EF Core
    public string Name { get; set; }
    public string City { get; set; }
    public int? DivisionId { get; set; }  // Foreign key (nullable for backwards compatibility)
    public List<Player> Players { get; set; } = new();  // EF Core navigation property
    public int Budget { get; set; }
    public int Championships { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int FanSupport { get; set; }      // 0-100
    public int Chemistry { get; set; }       // 0-100
    public Dictionary<TeamStatType, int> Stats { get; set; } = new();

    // Depth charts for different units
    public DepthChart OffenseDepthChart { get; set; } = new();
    public DepthChart DefenseDepthChart { get; set; } = new();
    public DepthChart FieldGoalOffenseDepthChart { get; set; } = new();
    public DepthChart FieldGoalDefenseDepthChart { get; set; } = new();
    public DepthChart KickoffOffenseDepthChart { get; set; } = new();
    public DepthChart KickoffDefenseDepthChart { get; set; } = new();
    public DepthChart PuntOffenseDepthChart { get; set; } = new();
    public DepthChart PuntDefenseDepthChart { get; set; } = new();

    // NFL-style coaching staff
    public Coach HeadCoach { get; set; } = new();
    public Coach OffensiveCoordinator { get; set; } = new();
    public Coach DefensiveCoordinator { get; set; } = new();
    public Coach SpecialTeamsCoordinator { get; set; } = new();
    public List<Coach> AssistantCoaches { get; set; } = new();

    // Training staff
    public Trainer HeadAthleticTrainer { get; set; } = new();
    public Trainer TeamDoctor { get; set; } = new();

    // Scouting staff
    public Scout DirectorOfScouting { get; set; } = new();
    public List<Scout> CollegeScouts { get; set; } = new();
    public List<Scout> ProScouts { get; set; } = new();

    // Team stats
    public Dictionary<string, int> TeamStats { get; set; } = new();
}
