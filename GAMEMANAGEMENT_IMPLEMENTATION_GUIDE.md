# GameManagement Implementation Guide

## Quick Reference for Building GameManagement Features

### Current Status
- **Partially Complete:** PlayerGeneratorService (basic implementation)
- **Interface Only:** ITeamBuilderService, IPlayerProgressionService
- **Not Started:** Coaching AI, Scouting System, Draft, Season Management

---

## 1. KEY DEPENDENCY INJECTION PATTERNS

### GameManagement.csproj Dependencies
```xml
<ProjectReference Include="..\DomainObjects\DomainObjects.csproj" />
<ProjectReference Include="..\DataAccessLayer\DataAccessLayer.csproj" />
```

### How to Use Repositories in GameManagement Services
```csharp
public class MyGameManagementService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    
    public MyGameManagementService(
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
    }
    
    // ALWAYS use repositories for database access
    public async Task<Team> GetTeamAsync(int teamId)
    {
        return await _teamRepository.GetByIdWithPlayersAsync(teamId);
    }
}
```

---

## 2. ARCHITECTURAL BOUNDARIES & CONSTRAINTS

### ✅ ALLOWED IN GAMEMANAGEMENT
- Reference DomainObjects models
- Use DataAccessLayer repositories
- Call logging services
- Perform business logic
- Validate data

### ❌ FORBIDDEN IN GAMEMANAGEMENT
- Direct EntityFrameworkCore usage
- Direct GridironDbContext references
- Writing SQL queries
- Using `.Include()`, `.ToListAsync()` directly
- Bypassing repositories

### RULE: Only DataAccessLayer talks to database
If you need a new database query:
1. Add method to repository interface
2. Implement in repository class
3. Use repository method in GameManagement

---

## 3. KEY CLASSES TO LEVERAGE

### Player Model
```csharp
// Key attributes for player evaluation
player.Speed          // 0-100
player.Strength       // 0-100
player.Agility        // 0-100
player.Awareness      // 0-100
player.Fragility      // 0-100 (higher = injury-prone)
player.Position       // Positions enum (QB, RB, WR, etc.)
player.Potential      // 0-100 (ceiling)
player.Progression    // 0-100 (development rate)
player.Health         // 0-100
player.Discipline     // 0-100 (affects penalties)
player.CurrentInjury  // Injury? (null = not injured)
player.IsInjured      // bool (convenience property)

// Position-specific skills
player.Passing        // QB primary
player.Catching       // WR, TE, RB
player.Rushing        // RB, QB
player.Blocking       // OL, TE
player.Tackling       // Defense
player.Coverage       // DB
player.Kicking        // K, P
```

### Team Model
```csharp
// Access players
team.Players           // List<Player>
team.OffenseDepthChart // Select players by position
team.DefenseDepthChart

// Team attributes
team.Chemistry         // 0-100
team.FanSupport        // 0-100
team.Budget            // Financial resources
team.Wins              // Win-loss record
team.Losses
team.Ties

// Coaching staff
team.HeadCoach
team.OffensiveCoordinator
team.DefensiveCoordinator
team.SpecialTeamsCoordinator
```

### DepthChart Usage
```csharp
// Get all QBs on team
var qbs = team.OffenseDepthChart.Chart[Positions.QB];
var starter = qbs[0];              // First is starter
var backup = qbs.Count > 1 ? qbs[1] : null;

// Iterate all positions
foreach (var position in team.OffenseDepthChart.Chart.Keys)
{
    var players = team.OffenseDepthChart.Chart[position];
    // Process position group
}
```

---

## 4. PLAYER GENERATION SERVICE (PARTIAL EXAMPLE)

### Current Implementation (`PlayerGeneratorService.cs`)

**What Exists:**
- `GenerateRandomPlayer(Positions)` - Create individual player
- `GenerateDraftClass(int year)` - Generate draft prospects
- `GenerateMultiplePlayers(int count)` - Batch generation
- Position-specific attribute ranges
- Jersey number assignment
- Height/weight generation

**What's Used By:**
- Tests for creating test players
- Draft system (when implemented)

**How to Extend:**
```csharp
// Example: Generate team roster
public List<Player> GenerateNFLRoster()
{
    var roster = new List<Player>();
    
    // Offensive positions (11 starters + depth)
    roster.AddRange(GeneratePositionGroup(Positions.QB, 2));
    roster.AddRange(GeneratePositionGroup(Positions.RB, 3));
    roster.AddRange(GeneratePositionGroup(Positions.FB, 1));
    roster.AddRange(GeneratePositionGroup(Positions.WR, 4));
    roster.AddRange(GeneratePositionGroup(Positions.TE, 2));
    roster.AddRange(GeneratePositionGroup(Positions.C, 2));
    roster.AddRange(GeneratePositionGroup(Positions.G, 4));
    roster.AddRange(GeneratePositionGroup(Positions.T, 4));
    
    // Defensive positions
    roster.AddRange(GeneratePositionGroup(Positions.DE, 3));
    roster.AddRange(GeneratePositionGroup(Positions.DT, 3));
    roster.AddRange(GeneratePositionGroup(Positions.LB, 4));
    roster.AddRange(GeneratePositionGroup(Positions.OLB, 2));
    roster.AddRange(GeneratePositionGroup(Positions.CB, 4));
    roster.AddRange(GeneratePositionGroup(Positions.S, 3));
    roster.AddRange(GeneratePositionGroup(Positions.FS, 1));
    
    // Special teams
    roster.AddRange(GeneratePositionGroup(Positions.K, 1));
    roster.AddRange(GeneratePositionGroup(Positions.P, 1));
    roster.AddRange(GeneratePositionGroup(Positions.LS, 1));
    
    return roster;
}

private List<Player> GeneratePositionGroup(Positions position, int count)
{
    var players = new List<Player>();
    for (int i = 0; i < count; i++)
    {
        players.Add(GenerateRandomPlayer(position));
    }
    return players;
}
```

---

## 5. TEAM BUILDER SERVICE (INTERFACE SKELETON)

### Current State: Interface Only
```csharp
public interface ITeamBuilderService
{
    Team CreateTeam(string city, string name, decimal budget);
    bool AddPlayerToTeam(Team team, Player player);
    void AssignDepthCharts(Team team);
    bool ValidateRoster(Team team);
}
```

### Implementation Example
```csharp
public class TeamBuilderService : ITeamBuilderService
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<TeamBuilderService> _logger;
    
    public TeamBuilderService(
        IPlayerRepository playerRepository,
        ILogger<TeamBuilderService> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }
    
    public Team CreateTeam(string city, string name, decimal budget)
    {
        var team = new Team
        {
            City = city,
            Name = name,
            Budget = (int)budget,
            Players = new List<Player>(),
            Chemistry = 50,      // Default neutral chemistry
            FanSupport = 50,      // Default neutral fan support
            HeadCoach = new Coach { Role = "Head Coach" },
            // Initialize other staff...
        };
        return team;
    }
    
    public bool AddPlayerToTeam(Team team, Player player)
    {
        // NFL rosters can have max 53 players
        if (team.Players.Count >= 53)
        {
            _logger.LogWarning("Cannot add {Player} - roster is full", player.FirstName);
            return false;
        }
        
        player.TeamId = team.Id;
        team.Players.Add(player);
        _logger.LogInformation("Added {Player} to {Team}", player.FirstName, team.Name);
        return true;
    }
    
    public void AssignDepthCharts(Team team)
    {
        // Build offensive depth chart
        team.OffenseDepthChart = BuildDepthChart(team, offense: true);
        
        // Build defensive depth chart
        team.DefenseDepthChart = BuildDepthChart(team, offense: false);
        
        // Build special team depth charts
        team.KickoffOffenseDepthChart = BuildSpecialTeamsChart(team);
        team.KickoffDefenseDepthChart = BuildSpecialTeamsChart(team);
        // ... other special team charts
    }
    
    private DepthChart BuildDepthChart(Team team, bool offense)
    {
        var chart = new DepthChart();
        var positions = offense ? GetOffensivePositions() : GetDefensivePositions();
        
        foreach (var position in positions)
        {
            var playersAtPosition = team.Players
                .Where(p => p.Position == position)
                .OrderByDescending(p => CalculatePlayerRating(p))
                .ToList();
            
            chart.Chart[position] = playersAtPosition;
        }
        
        return chart;
    }
    
    public bool ValidateRoster(Team team)
    {
        // Validation rules
        var hasQB = team.Players.Any(p => p.Position == Positions.QB);
        var hasRB = team.Players.Any(p => p.Position == Positions.RB);
        var hasWR = team.Players.Any(p => p.Position == Positions.WR);
        var hasDefense = team.Players.Any(p => IsDefensivePosition(p.Position));
        
        return hasQB && hasRB && hasWR && hasDefense;
    }
}
```

---

## 6. PLAYER PROGRESSION SERVICE (INTERFACE SKELETON)

### Current State: Interface Only
```csharp
public interface IPlayerProgressionService
{
    void ProgressPlayer(Player player);
    int CalculateOverallRating(Player player);
    void UpdatePlayerStats(Player player, Game game);
}
```

### Implementation Example
```csharp
public class PlayerProgressionService : IPlayerProgressionService
{
    private readonly ILogger<PlayerProgressionService> _logger;
    
    public void ProgressPlayer(Player player)
    {
        // Age increases
        player.Age++;
        
        // Skills improve based on progression rate
        var progressionRate = (player.Progression - 50) / 100.0; // -50 to +50 impact
        
        player.Speed = AdjustAttribute(player.Speed, progressionRate, player.Age);
        player.Strength = AdjustAttribute(player.Strength, progressionRate, player.Age);
        player.Agility = AdjustAttribute(player.Agility, progressionRate, player.Age);
        
        // Experience increases
        if (player.Exp < 15) // Cap at 15 years
        {
            player.Exp++;
        }
        
        // Potential decreases with age
        if (player.Age > 28)
        {
            player.Potential = Math.Max(0, player.Potential - 2);
        }
        
        _logger.LogInformation("{Player} progressed to age {Age}", player.FirstName, player.Age);
    }
    
    public int CalculateOverallRating(Player player)
    {
        // Weighted average of all attributes
        double rating = 0;
        double totalWeight = 0;
        
        // General attributes (0.5 weight)
        rating += player.Speed * 0.15;
        rating += player.Strength * 0.15;
        rating += player.Agility * 0.1;
        rating += player.Awareness * 0.1;
        
        // Position-specific (0.35 weight)
        rating += GetPositionSkillRating(player) * 0.35;
        
        // Experience bonus (0.1 weight)
        rating += Math.Min(player.Exp * 2, 20);
        
        return (int)Math.Round(rating);
    }
    
    public void UpdatePlayerStats(Player player, Game game)
    {
        // Track stats from plays where player participated
        foreach (var play in game.Plays)
        {
            // Count all plays player was in
            if (play.OffensePlayersOnField.Contains(player) || 
                play.DefensePlayersOnField.Contains(player))
            {
                player.Stats[PlayerStatType.PlaysParticipated]++;
            }
            
            // Track specific contributions based on play type
            // (This would be enhanced based on actual play details)
        }
    }
    
    private int AdjustAttribute(int current, double progression, int age)
    {
        var ageFactor = age > 28 ? 1 - ((age - 28) * 0.02) : 1.0;
        var change = (int)(progression * ageFactor);
        return Math.Max(0, Math.Min(100, current + change));
    }
}
```

---

## 7. COACHING AI (FUTURE)

### What Needs to Be Built
```csharp
public interface ICoachingService
{
    PlayType SelectOffensivePlay(Game game, Team offenseTeam, Team defenseTeam);
    PlayType SelectDefensivePackage(Game game, Team defenseTeam);
    bool CallTimeout(Game game, Team team);
    bool ChallengeCall(Game game, Team team);
}
```

### Current State
- Coach objects exist with attributes (Leadership, Strategy, Motivation, Adaptability)
- No play-calling logic
- Currently: 50/50 random Run vs Pass selection

### Implementation Would Need
- Situation-based play calling
- Field position consideration
- Game state (score, time, down & distance)
- Team strengths/weaknesses
- Opponent tendencies

---

## 8. DRAFT SYSTEM (FUTURE)

### What Would Be Needed
```csharp
public interface IDraftService
{
    List<DraftPick> RunDraft(int year, List<Team> teams);
    Player EvaluateDraftProspect(Player prospect);
    decimal CalculatePlayerValue(Player player);
}
```

### Key Concepts
- Generate draft prospects (PlayerGeneratorService already has `GenerateDraftClass()`)
- Track draft picks
- Assign drafted players to teams
- Handle trades
- Contract assignment

---

## 9. SEASON MANAGEMENT (FUTURE)

### What Would Be Needed
```csharp
public interface ISeasonService
{
    Season CreateSeason(int year);
    void ScheduleGames(Season season);
    void SimulateWeek(Season season, int week);
    void UpdateStandings(Season season);
    void RunPlayoffs(Season season);
}
```

---

## 10. TESTING GAMEMANAGEMENT SERVICES

### Test Structure Pattern
```csharp
[TestClass]
public class TeamBuilderServiceTests
{
    private Mock<IPlayerRepository> _mockPlayerRepository;
    private Mock<ILogger<TeamBuilderService>> _mockLogger;
    private TeamBuilderService _service;
    
    [TestInitialize]
    public void Setup()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockLogger = new Mock<ILogger<TeamBuilderService>>();
        
        _service = new TeamBuilderService(
            _mockPlayerRepository.Object,
            _mockLogger.Object);
    }
    
    [TestMethod]
    public void CreateTeam_WithValidData_CreatesTeamSuccessfully()
    {
        // Arrange
        var city = "Atlanta";
        var name = "Falcons";
        var budget = 250000000m;
        
        // Act
        var team = _service.CreateTeam(city, name, budget);
        
        // Assert
        Assert.AreEqual(city, team.City);
        Assert.AreEqual(name, team.Name);
        Assert.AreEqual((int)budget, team.Budget);
        Assert.AreEqual(0, team.Players.Count);
    }
    
    [TestMethod]
    public void AddPlayerToTeam_WithFullRoster_ReturnsFalse()
    {
        // Arrange
        var team = new Team { Players = new List<Player>() };
        for (int i = 0; i < 53; i++)
        {
            team.Players.Add(new Player { Id = i });
        }
        var newPlayer = new Player { Id = 54 };
        
        // Act
        var result = _service.AddPlayerToTeam(team, newPlayer);
        
        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(53, team.Players.Count);
    }
}
```

---

## 11. FILE LOCATIONS REFERENCE

### Files You'll Be Working With
```
GameManagement/
├── Services/
│   ├── ITeamBuilderService.cs          (interface to implement)
│   ├── IPlayerGeneratorService.cs      (interface, partial impl)
│   ├── PlayerGeneratorService.cs       (partial impl)
│   ├── IPlayerProgressionService.cs    (interface to implement)
│   └── [New services go here]
│
├── Helpers/
│   ├── OverallRatingCalculator.cs      (use for player ratings)
│   └── [New helpers go here]
│
├── TestData/
│   ├── FirstNames.json
│   ├── LastNames.json
│   └── Colleges.json
│
└── GameManagement.csproj

GameManagement.Tests/
├── [test files]
└── GameManagement.Tests.csproj
```

---

## 12. KEY HELPER CLASSES TO USE

### OverallRatingCalculator
```csharp
// Already implemented
var overall = OverallRatingCalculator.Calculate(player);
var salary = OverallRatingCalculator.CalculateSalary(player, overall);
```

### Teams Helper (for depth charts)
```csharp
// Builds depth charts automatically
var teams = new Teams(homeTeam, awayTeam);
var homeWithCharts = teams.HomeTeam;   // Has populated depth charts
var awayWithCharts = teams.VisitorTeam;
```

### DepthChart Usage
```csharp
// Get players by position
var qbs = team.OffenseDepthChart.Chart[Positions.QB];
var starter = qbs[0];
var backup = qbs.Count > 1 ? qbs[1] : null;
```

---

## 13. DO'S AND DON'TS

### DO
- ✅ Inject repositories as dependencies
- ✅ Use async/await for database calls
- ✅ Log meaningful messages
- ✅ Validate input data
- ✅ Write unit tests with mocked repositories
- ✅ Use existing Player/Team/Coach models
- ✅ Reference ARCHITECTURE_PRINCIPLES.md for data access rules

### DON'T
- ❌ Use GridironDbContext directly
- ❌ Use EF Core Include(), ToListAsync(), etc. directly
- ❌ Write raw SQL
- ❌ Ignore the repository pattern
- ❌ Create breaking changes to existing models
- ❌ Skip testing
- ❌ Modify DataAccessLayer without updating repository interfaces

---

## 14. NEXT STEPS FOR IMPLEMENTATION

1. **Start with ITeamBuilderService Implementation**
   - Implement CreateTeam()
   - Implement AddPlayerToTeam()
   - Implement AssignDepthCharts()
   - Implement ValidateRoster()
   - Add tests

2. **Complete IPlayerProgressionService**
   - Implement ProgressPlayer()
   - Implement CalculateOverallRating()
   - Implement UpdatePlayerStats()
   - Add tests

3. **Enhance PlayerGeneratorService**
   - Add GenerateNFLRoster()
   - Add GenerateCoachingStaff()
   - Add GenerateMedicalStaff()

4. **Plan Coaching AI**
   - Define play-calling strategy interface
   - Implement situation-based play selection
   - Add timeout/challenge logic

5. **Plan Draft System**
   - Define draft service interfaces
   - Implement prospect evaluation
   - Implement pick assignment

6. **Plan Season Management**
   - Define season entities
   - Implement scheduling
   - Implement standings tracking

---

## SUMMARY

You now have:
1. Understanding of the complete architecture
2. All domain models (Player, Team, Coach, etc.)
3. Access to repositories for data persistence
4. Test infrastructure and patterns
5. Partial implementations to build upon
6. Clear architectural constraints and rules

Start with completing the Team Builder and Player Progression services, then move to more complex features like coaching AI and season management.

