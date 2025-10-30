using DomainObjects;
using static DomainObjects.StatTypes;

public class Team
{
    public string Name { get; set; }
    public string City { get; set; }
    public List<Player> Players { get; set; } = new();
    public int Budget { get; set; }
    public int Championships { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int FanSupport { get; set; }      // 0-100
    public int Chemistry { get; set; }       // 0-100
    public Dictionary<TeamStatType, int> Stats { get; set; } = new();

    // NFL-style coaching staff
    public Coach HeadCoach { get; set; }
    public Coach OffensiveCoordinator { get; set; }
    public Coach DefensiveCoordinator { get; set; }
    public Coach SpecialTeamsCoordinator { get; set; }
    public List<Coach> AssistantCoaches { get; set; } = new();

    // Training staff
    public Trainer HeadAthleticTrainer { get; set; }
    public Trainer TeamDoctor { get; set; }

    // Scouting staff
    public Scout DirectorOfScouting { get; set; }
    public List<Scout> CollegeScouts { get; set; } = new();
    public List<Scout> ProScouts { get; set; } = new();

    // Team stats
    public Dictionary<string, int> TeamStats { get; set; } = new();
}