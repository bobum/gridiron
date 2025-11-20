using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing teams
/// </summary>
public class TeamBuilderService : ITeamBuilderService
{
    private readonly ILogger<TeamBuilderService> _logger;
    private const int MaxRosterSize = 53;  // NFL roster limit

    public TeamBuilderService(ILogger<TeamBuilderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Team CreateTeam(string city, string name, decimal budget)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (budget < 0)
            throw new ArgumentException("Budget cannot be negative", nameof(budget));

        var team = new Team
        {
            City = city,
            Name = name,
            Budget = (int)budget,
            Players = new List<Player>(),

            // Initialize default values
            Championships = 0,
            Wins = 0,
            Losses = 0,
            Ties = 0,
            FanSupport = 50,  // Start at 50% fan support
            Chemistry = 50,   // Start at 50% chemistry

            // Initialize empty depth charts (will be populated by AssignDepthCharts)
            OffenseDepthChart = new DepthChart(),
            DefenseDepthChart = new DepthChart(),
            FieldGoalOffenseDepthChart = new DepthChart(),
            FieldGoalDefenseDepthChart = new DepthChart(),
            KickoffOffenseDepthChart = new DepthChart(),
            KickoffDefenseDepthChart = new DepthChart(),
            PuntOffenseDepthChart = new DepthChart(),
            PuntDefenseDepthChart = new DepthChart(),

            // Initialize coaching staff with default coaches
            HeadCoach = new Coach { FirstName = "Head", LastName = "Coach", Role = "Head Coach" },
            OffensiveCoordinator = new Coach { FirstName = "Offensive", LastName = "Coordinator", Role = "Offensive Coordinator" },
            DefensiveCoordinator = new Coach { FirstName = "Defensive", LastName = "Coordinator", Role = "Defensive Coordinator" },
            SpecialTeamsCoordinator = new Coach { FirstName = "Special Teams", LastName = "Coordinator", Role = "Special Teams Coordinator" },
            AssistantCoaches = new List<Coach>(),

            // Initialize training staff
            HeadAthleticTrainer = new Trainer { FirstName = "Head", LastName = "Trainer", Role = "Head Athletic Trainer" },
            TeamDoctor = new Trainer { FirstName = "Team", LastName = "Doctor", Role = "Team Doctor" },

            // Initialize scouting staff
            DirectorOfScouting = new Scout { FirstName = "Director", LastName = "Scouting", Role = "Director of Scouting" },
            CollegeScouts = new List<Scout>(),
            ProScouts = new List<Scout>(),

            Stats = new Dictionary<StatTypes.TeamStatType, int>(),
            TeamStats = new Dictionary<string, int>()
        };

        _logger.LogInformation("Created new team: {City} {Name} with budget ${Budget:N0}",
            city, name, budget);

        return team;
    }

    public bool AddPlayerToTeam(Team team, Player player)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        if (player == null)
            throw new ArgumentNullException(nameof(player));

        // Check roster limit
        if (team.Players.Count >= MaxRosterSize)
        {
            _logger.LogWarning("Cannot add player {FirstName} {LastName} to {City} {Name}: Roster is full ({Count}/{Max})",
                player.FirstName, player.LastName, team.City, team.Name, team.Players.Count, MaxRosterSize);
            return false;
        }

        // Add player to team
        player.TeamId = team.Id;
        team.Players.Add(player);

        _logger.LogInformation("Added player {FirstName} {LastName} ({Position}) to {City} {Name}. Roster: {Count}/{Max}",
            player.FirstName, player.LastName, player.Position, team.City, team.Name, team.Players.Count, MaxRosterSize);

        return true;
    }

    public void AssignDepthCharts(Team team)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        if (team.Players == null || team.Players.Count == 0)
        {
            _logger.LogWarning("Cannot assign depth charts for {City} {Name}: No players on roster",
                team.City, team.Name);
            return;
        }

        // Use the Teams helper class to build all depth charts
        // This is the same logic used by the game simulation engine
        team.OffenseDepthChart = BuildOffenseDepthChart(team.Players);
        team.DefenseDepthChart = BuildDefenseDepthChart(team.Players);
        team.FieldGoalOffenseDepthChart = BuildFieldGoalOffenseDepthChart(team.Players);
        team.FieldGoalDefenseDepthChart = BuildFieldGoalDefenseDepthChart(team.Players);
        team.KickoffOffenseDepthChart = BuildKickoffOffenseDepthChart(team.Players);
        team.KickoffDefenseDepthChart = BuildKickoffDefenseDepthChart(team.Players);
        team.PuntOffenseDepthChart = BuildPuntOffenseDepthChart(team.Players);
        team.PuntDefenseDepthChart = BuildPuntDefenseDepthChart(team.Players);

        _logger.LogInformation("Assigned depth charts for {City} {Name} with {PlayerCount} players",
            team.City, team.Name, team.Players.Count);
    }

    public bool ValidateRoster(Team team)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        var issues = new List<string>();

        // Check roster size
        if (team.Players.Count < 22)
        {
            issues.Add($"Roster has only {team.Players.Count} players (minimum 22 for offense/defense starters)");
        }

        if (team.Players.Count > MaxRosterSize)
        {
            issues.Add($"Roster has {team.Players.Count} players (maximum {MaxRosterSize})");
        }

        // Check for minimum required positions
        var requiredPositions = new Dictionary<Positions, int>
        {
            { Positions.QB, 1 },
            { Positions.RB, 1 },
            { Positions.WR, 2 },
            { Positions.TE, 1 },
            { Positions.C, 1 },
            { Positions.G, 2 },
            { Positions.T, 2 },
            { Positions.DE, 2 },
            { Positions.DT, 2 },
            { Positions.LB, 2 },
            { Positions.CB, 2 },
            { Positions.S, 2 },
            { Positions.K, 1 },
            { Positions.P, 1 }
        };

        foreach (var (position, minCount) in requiredPositions)
        {
            var count = team.Players.Count(p => p.Position == position);
            if (count < minCount)
            {
                issues.Add($"Position {position} has {count} players (minimum {minCount})");
            }
        }

        // Log validation results
        if (issues.Any())
        {
            _logger.LogWarning("Roster validation failed for {City} {Name}:\n{Issues}",
                team.City, team.Name, string.Join("\n", issues.Select(i => $"  - {i}")));
            return false;
        }

        _logger.LogInformation("Roster validation passed for {City} {Name} with {PlayerCount} players",
            team.City, team.Name, team.Players.Count);
        return true;
    }

    // Private helper methods for building depth charts
    // These mirror the logic in DomainObjects.Helpers.Teams but are instance methods

    private int GetPositionSkill(Player p, Positions pos)
    {
        return pos switch
        {
            Positions.QB => p.Passing,
            Positions.RB => p.Rushing,
            Positions.FB => p.Blocking,
            Positions.WR => p.Catching,
            Positions.TE => p.Catching + p.Blocking,
            Positions.C => p.Blocking,
            Positions.G => p.Blocking,
            Positions.T => p.Blocking,
            Positions.DE => p.Tackling + p.Agility,
            Positions.DT => p.Tackling + p.Strength,
            Positions.LB => p.Tackling + p.Coverage,
            Positions.CB => p.Coverage + p.Speed,
            Positions.S => p.Coverage + p.Tackling,
            Positions.FS => p.Coverage + p.Tackling,
            Positions.K => p.Kicking,
            Positions.P => p.Kicking,
            Positions.LS => p.Blocking,
            _ => 0
        };
    }

    private List<Player> GetDepth(List<Player> players, Positions pos, int depth = 1)
    {
        return players
            .Where(p => p.Position == pos)
            .OrderByDescending(p => GetPositionSkill(p, pos))
            .Take(depth)
            .ToList();
    }

    private DepthChart BuildOffenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.QB] = GetDepth(players, Positions.QB);
        chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
        chart.Chart[Positions.FB] = GetDepth(players, Positions.FB);
        chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 3);
        chart.Chart[Positions.TE] = GetDepth(players, Positions.TE);
        chart.Chart[Positions.C] = GetDepth(players, Positions.C);
        chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);
        chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);
        return chart;
    }

    private DepthChart BuildDefenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.DE] = GetDepth(players, Positions.DE, 2);
        chart.Chart[Positions.DT] = GetDepth(players, Positions.DT, 2);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 4);
        chart.Chart[Positions.OLB] = GetDepth(players, Positions.OLB, 2);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        chart.Chart[Positions.FS] = GetDepth(players, Positions.FS, 1);
        return chart;
    }

    private DepthChart BuildFieldGoalOffenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.K] = GetDepth(players, Positions.K, 1);
        chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);
        chart.Chart[Positions.H] = GetDepth(players, Positions.QB, 1);
        chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);
        chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);
        chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 2);
        chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
        return chart;
    }

    private DepthChart BuildFieldGoalDefenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.DE] = GetDepth(players, Positions.DE, 2);
        chart.Chart[Positions.DT] = GetDepth(players, Positions.DT, 2);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 3);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        return chart;
    }

    private DepthChart BuildKickoffOffenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.K] = GetDepth(players, Positions.K, 1);
        chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);
        chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
        return chart;
    }

    private DepthChart BuildKickoffDefenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);
        chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);
        chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 1);
        chart.Chart[Positions.G] = GetDepth(players, Positions.G, 1);
        chart.Chart[Positions.FB] = GetDepth(players, Positions.FB, 1);
        return chart;
    }

    private DepthChart BuildPuntOffenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.P] = GetDepth(players, Positions.P, 1);
        chart.Chart[Positions.LS] = GetDepth(players, Positions.LS, 1);
        chart.Chart[Positions.G] = GetDepth(players, Positions.G, 2);
        chart.Chart[Positions.T] = GetDepth(players, Positions.T, 2);
        chart.Chart[Positions.C] = GetDepth(players, Positions.C, 1);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        return chart;
    }

    private DepthChart BuildPuntDefenseDepthChart(List<Player> players)
    {
        var chart = new DepthChart();
        chart.Chart[Positions.WR] = GetDepth(players, Positions.WR, 2);
        chart.Chart[Positions.RB] = GetDepth(players, Positions.RB, 2);
        chart.Chart[Positions.CB] = GetDepth(players, Positions.CB, 2);
        chart.Chart[Positions.S] = GetDepth(players, Positions.S, 2);
        chart.Chart[Positions.LB] = GetDepth(players, Positions.LB, 2);
        chart.Chart[Positions.TE] = GetDepth(players, Positions.TE, 1);
        chart.Chart[Positions.G] = GetDepth(players, Positions.G, 1);
        chart.Chart[Positions.FB] = GetDepth(players, Positions.FB, 1);
        return chart;
    }
}
