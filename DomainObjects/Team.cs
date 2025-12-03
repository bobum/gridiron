using DomainObjects;
using static DomainObjects.StatTypes;

public class Team : SoftDeletableEntity
{
    public int Id { get; set; } // Primary key for EF Core

    public required string Name { get; set; }

    public string? City { get; set; }

    public int? DivisionId { get; set; } // Foreign key (nullable for backwards compatibility)

    public List<Player> Players { get; set; } = new ();  // EF Core navigation property

    public int Budget { get; set; }

    public int Championships { get; set; }

    public int Wins { get; set; }

    public int Losses { get; set; }

    public int Ties { get; set; }

    public int FanSupport { get; set; } // 0-100

    public int Chemistry { get; set; } // 0-100

    public Dictionary<TeamStatType, int> Stats { get; set; } = new ();

    // Depth charts for different units
    public DepthChart OffenseDepthChart { get; set; } = new ();

    public DepthChart DefenseDepthChart { get; set; } = new ();

    public DepthChart FieldGoalOffenseDepthChart { get; set; } = new ();

    public DepthChart FieldGoalDefenseDepthChart { get; set; } = new ();

    public DepthChart KickoffOffenseDepthChart { get; set; } = new ();

    public DepthChart KickoffDefenseDepthChart { get; set; } = new ();

    public DepthChart PuntOffenseDepthChart { get; set; } = new ();

    public DepthChart PuntDefenseDepthChart { get; set; } = new ();

    // NFL-style coaching staff (nullable - positions may be unfilled)
    public Coach? HeadCoach { get; set; }

    public Coach? OffensiveCoordinator { get; set; }

    public Coach? DefensiveCoordinator { get; set; }

    public Coach? SpecialTeamsCoordinator { get; set; }

    public List<Coach> AssistantCoaches { get; set; } = new ();

    // Training staff (nullable - positions may be unfilled)
    public Trainer? HeadAthleticTrainer { get; set; }

    public Trainer? TeamDoctor { get; set; }

    // Scouting staff (nullable - positions may be unfilled)
    public Scout? DirectorOfScouting { get; set; }

    public List<Scout> CollegeScouts { get; set; } = new ();

    public List<Scout> ProScouts { get; set; } = new ();

    // Team stats
    public Dictionary<string, int> TeamStats { get; set; } = new ();
}
