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
    private readonly IPlayerGeneratorService _playerGenerator;
    private const int MaxRosterSize = 53;  // NFL roster limit

    public TeamBuilderService(
        ILogger<TeamBuilderService> logger,
        IPlayerGeneratorService playerGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _playerGenerator = playerGenerator ?? throw new ArgumentNullException(nameof(playerGenerator));
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

        // Use centralized DepthChartBuilder for all depth chart logic
        // Same logic used by game simulation engine
        DepthChartBuilder.AssignAllDepthCharts(team);

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

    public Team PopulateTeamRoster(Team team, int? seed = null)
    {
        if (team == null)
            throw new ArgumentNullException(nameof(team));

        _logger.LogInformation("Populating roster for {City} {Name} (seed: {Seed})",
            team.City, team.Name, seed?.ToString() ?? "random");

        // Clear existing players
        team.Players.Clear();

        // Define NFL standard 53-man roster composition
        var rosterComposition = new Dictionary<Positions, int>
        {
            { Positions.QB, 2 },
            { Positions.RB, 4 },
            { Positions.FB, 1 },
            { Positions.WR, 6 },
            { Positions.TE, 3 },
            { Positions.C, 2 },
            { Positions.G, 4 },
            { Positions.T, 4 },
            { Positions.DE, 4 },
            { Positions.DT, 3 },
            { Positions.LB, 4 },
            { Positions.OLB, 2 },
            { Positions.CB, 5 },
            { Positions.S, 3 },
            { Positions.FS, 2 },
            { Positions.K, 1 },
            { Positions.P, 1 },
            { Positions.LS, 1 },
            { Positions.H, 1 }
        };

        // Generate players for each position
        int playerIndex = 0;
        foreach (var (position, count) in rosterComposition)
        {
            for (int i = 0; i < count; i++)
            {
                // Use seed + index for reproducible but varied generation
                var playerSeed = seed.HasValue ? seed.Value + playerIndex : (int?)null;
                var player = _playerGenerator.GenerateRandomPlayer(position, playerSeed);

                // Assign to team
                player.TeamId = team.Id;
                team.Players.Add(player);

                playerIndex++;
            }
        }

        _logger.LogInformation("Successfully populated roster for {City} {Name} with {Count} players",
            team.City, team.Name, team.Players.Count);

        // Assign depth charts after roster is populated
        AssignDepthCharts(team);

        return team;
    }
}
