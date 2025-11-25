# GRIDIRON PROJECT - COMPREHENSIVE ARCHITECTURE OVERVIEW

## Executive Summary

Gridiron is a sophisticated NFL football game simulation engine with a React frontend and .NET 8 backend. The backend uses a state machine architecture to simulate realistic football games with comprehensive player attributes, statistical modeling, penalty systems, injury tracking, and database persistence. The codebase is well-structured with clear separation of concerns across multiple projects.

**Key Facts:**
- **Status:** Actively maintained (November 2025)
- **Backend:** 140MB, 229 C# files, 40,800+ lines of code, 22,200+ lines of test code
- **Frontend:** React 18 + TypeScript with Vite, TailwindCSS, TanStack Query
- **Test Coverage:** 100% passing (839/839 backend tests, 25 frontend tests)
- **CI/CD:** GitHub Actions with automated testing on every PR
- **Architecture:** Clean layered architecture with clear separation between frontend, API, domain, simulation, and persistence layers

---

## 1. OVERALL PROJECT STRUCTURE & PROJECT RELATIONSHIPS

### 1.1 Project Dependency Graph

```
┌─────────────────────────────────────────────────────────────┐
│                   Gridiron.WebApi (ASP.NET Core API)         │
│  - REST controllers for game simulation and team management  │
│  - Depends on: DataAccessLayer, StateLibrary, DomainObjects │
└──────────────────┬──────────────────────────────────────────┘
                   │
┌──────────────────┴──────────────────────────────────────────┐
│                   DataAccessLayer                           │
│  - Entity Framework Core DbContext                          │
│  - Repository Pattern (ITeamRepository, IPlayerRepository,  │
│    IGameRepository)                                         │
│  - Database seed data loaders                               │
│  - Depends on: DomainObjects                                │
└──────────────────┬──────────────────────────────────────────┘
                   │
┌──────────────────┴──────────────────────────────────────────┐
│                                                              │
│  ┌─────────────────┐      ┌──────────────────────────────┐ │
│  │ StateLibrary    │      │    GridironConsole          │ │
│  │ (Game Engine)   │      │    (Console Simulation)     │ │
│  │ - GameFlow      │      │                             │ │
│  │ - Play logic    │      │ Depends on: StateLibrary,   │ │
│  │ - Skills checks │      │ DomainObjects, DataAccessL.│ │
│  │ - Penalty sys.  │      └──────────────────────────────┘ │
│  │ - Injury sys.   │                                       │
│  │ Depends on:     │      ┌──────────────────────────────┐ │
│  │ DomainObjects   │      │    GameManagement           │ │
│  └─────────────────┘      │    (Player/Team Services)   │ │
│                           │                             │ │
│                           │ Depends on: DomainObjects,  │ │
│                           │ DataAccessLayer             │ │
│                           └──────────────────────────────┘ │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            DomainObjects (Models)                    │  │
│  │ - Player, Team, Game, Coach, Trainer                │  │
│  │ - Play types (Run, Pass, Kickoff, Punt, FieldGoal) │  │
│  │ - Injury, Penalty, Fumble, Interception            │  │
│  │ - Enums: Positions, Downs, Possession, PlayType    │  │
│  │ - Helpers: SeedableRandom, GameHelper, Teams       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│              Test Projects (NOT in dependency chain)         │
│                                                              │
│  - UnitTestProject1 (839 tests)                             │
│    Depends on: DomainObjects, StateLibrary, Helpers        │
│                                                              │
│  - Gridiron.WebApi.Tests                                    │
│    Depends on: Gridiron.WebApi, Mocking libraries          │
│                                                              │
│  - GameManagement.Tests                                     │
│    Depends on: GameManagement                              │
└──────────────────────────────────────────────────────────────┘
```

### 1.2 Frontend Architecture (gridiron-web)

```
┌──────────────────────────────────────────────────────────────┐
│                   React Frontend (gridiron-web)              │
│  - Built with Vite + React 18 + TypeScript                  │
│  - TailwindCSS for styling                                   │
│  - TanStack Query for data fetching and caching             │
│  - Axios for HTTP requests to Gridiron.WebApi               │
│                                                              │
│  Components:                                                 │
│    - Navigation: Header nav bar with routing               │
│    - Loading: Reusable loading spinner                     │
│    - ErrorMessage: Error state display                     │
│    - Team components: Team lists and details               │
│                                                              │
│  Pages:                                                      │
│    - HomePage: API status and quick actions                │
│    - TeamsPage: Browse all teams                            │
│    - GameSimulationPage: Simulate games between teams      │
│                                                              │
│  API Client (src/api/):                                     │
│    - client.ts: Axios instance with interceptors           │
│    - teams.ts: useTeams() hook                             │
│    - games.ts: useSimulateGame() mutation                  │
│                                                              │
│  Testing:                                                    │
│    - Vitest + React Testing Library (15 component tests)   │
│    - MSW (Mock Service Worker) for API mocking            │
│    - Playwright E2E tests (10 tests with real API)        │
│                                                              │
│  Vite Proxy Configuration:                                  │
│    - /api/* → http://localhost:5000 (dev)                  │
│    - Seamless API integration during development          │
└──────────────────────────────────────────────────────────────┘
          ↓ HTTP Requests to /api/*
┌──────────────────────────────────────────────────────────────┐
│              Gridiron.WebApi (ASP.NET Core)                  │
│  REST endpoints: /api/teams, /api/games/simulate, etc.     │
└──────────────────────────────────────────────────────────────┘
```

### 1.3 The Architectural Rule: Repository Pattern

**CRITICAL ARCHITECTURE PRINCIPLE:**
> **Only the DataAccessLayer project may communicate with ANY database. No exceptions.**

- ✅ DataAccessLayer contains GridironDbContext and all repositories
- ❌ ALL other projects (WebApi, Console, GameManagement) MUST use repository interfaces
- ❌ Never reference GridironDbContext directly outside DataAccessLayer
- ❌ Never use EF Core methods directly outside DataAccessLayer

**Repositories Available:**
- `ITeamRepository` - Team CRUD and queries
- `IPlayerRepository` - Player CRUD and queries
- `IGameRepository` - Game CRUD and queries
- `IPlayerDataRepository` - Player generation data (FirstNames, LastNames, Colleges)

---

## 2. DOMAINOBJECTS PROJECT - CORE DATA MODELS

### 2.1 Core Entity Models

#### **Player Model** (`Player.cs`)
```
Player : Person
├─ Id (EF Key)
├─ TeamId (Foreign Key, nullable)
├─ Position (QB, RB, WR, TE, OL, DL, LB, CB, S, K, P, LS, etc.)
├─ Number (Jersey number)
├─ Height, Weight, Age, Exp (Experience)
├─ College
│
├─ GENERAL ATTRIBUTES (0-100 scale)
│  ├─ Speed (0-100)
│  ├─ Strength (0-100)
│  ├─ Agility (0-100)
│  ├─ Awareness (0-100)
│  ├─ Fragility (0-100, default 50 - higher = injury-prone)
│  ├─ Morale (0-100)
│  ├─ Discipline (0-100, affects penalty rates)
│
├─ POSITION-SPECIFIC SKILLS
│  ├─ Passing (QB primary)
│  ├─ Catching (WR, TE, RB)
│  ├─ Rushing (RB, QB)
│  ├─ Blocking (OL, TE, FB)
│  ├─ Tackling (DL, LB, S, CB)
│  ├─ Coverage (CB, S, LB)
│  ├─ Kicking (K, P)
│
├─ CAREER TRACKING
│  ├─ ContractYears, Salary
│  ├─ Potential (0-100)
│  ├─ Progression (0-100)
│  ├─ Health (0-100)
│  ├─ IsRetired
│
├─ STATS TRACKING
│  ├─ Stats (Dictionary<PlayerStatType, int>)
│  ├─ SeasonStats (Dictionary<PlayerStatType, int>)
│  ├─ CareerStats (Dictionary<PlayerStatType, int>)
│
└─ INJURY TRACKING
   ├─ CurrentInjury (Injury? - null if not injured)
   └─ IsInjured (bool - read-only property)
```

**Key Relationships:**
- Many players → One team (1-to-many via TeamId)
- Player has 0 or 1 CurrentInjury
- Players selected for play from depth charts

#### **Team Model** (`Team.cs`)
```
Team
├─ Id (EF Key)
├─ Name, City
├─ Players (List<Player>, 1-to-many navigation property)
│
├─ FINANCIAL & RECORDS
│  ├─ Budget
│  ├─ Championships
│  ├─ Wins, Losses, Ties
│  ├─ FanSupport (0-100)
│  ├─ Chemistry (0-100)
│
├─ COACHING STAFF
│  ├─ HeadCoach
│  ├─ OffensiveCoordinator
│  ├─ DefensiveCoordinator
│  ├─ SpecialTeamsCoordinator
│  ├─ AssistantCoaches (List)
│
├─ MEDICAL STAFF
│  ├─ HeadAthleticTrainer
│  ├─ TeamDoctor
│
├─ SCOUTING STAFF
│  ├─ DirectorOfScouting
│  ├─ CollegeScouts (List)
│  ├─ ProScouts (List)
│
├─ DEPTH CHARTS (8 charts for different units)
│  ├─ OffenseDepthChart
│  ├─ DefenseDepthChart
│  ├─ FieldGoalOffenseDepthChart
│  ├─ FieldGoalDefenseDepthChart
│  ├─ KickoffOffenseDepthChart
│  ├─ KickoffDefenseDepthChart
│  ├─ PuntOffenseDepthChart
│  └─ PuntDefenseDepthChart
│
└─ STATS
   ├─ Stats (Dictionary<TeamStatType, int>)
   └─ TeamStats (Dictionary<string, int>)
```

**DepthChart Structure:**
```csharp
DepthChart
└─ Chart: Dictionary<Positions, List<Player>>
   // For each position: ordered list of players (starter first)
   // Example: Chart[Positions.QB] = [Tom Brady, Kyle Pitts, ...]
```

#### **Game Model** (`Game.cs`)
```
Game
├─ Id (EF Key)
├─ HomeTeamId, AwayTeamId (Foreign Keys)
├─ HomeTeam, AwayTeam (Navigation properties)
├─ RandomSeed (int?, for reproducible simulations)
│
├─ GAME STATE
│  ├─ CurrentPlay (IPlay)
│  ├─ Plays (List<IPlay>, all plays in game)
│  ├─ FieldPosition (0-100, absolute field position)
│  ├─ YardsToGo (10 by default)
│  ├─ CurrentDown (Downs enum)
│  ├─ WonCoinToss (Possession)
│  ├─ DeferredPossession (bool)
│
├─ SCORING
│  ├─ HomeScore
│  ├─ AwayScore
│
├─ TIMING
│  ├─ Halves (List<Half> - [FirstHalf, SecondHalf])
│  ├─ CurrentHalf
│  ├─ CurrentQuarter
│  ├─ TimeRemaining (calculated property)
│
└─ LOGGING
   └─ Logger (ILogger for play-by-play output)

SCORING METHODS:
├─ AddTouchdown(Possession)
├─ AddFieldGoal(Possession)
├─ AddSafety(Possession)  // Defending team gets points
├─ AddExtraPoint(Possession)
├─ AddTwoPointConversion(Possession)
└─ FormatFieldPosition(...) // NFL notation formatting
```

#### **Player Generation Data Models**

These entities support the player generation system by providing realistic name and college data:

**FirstName Model** (`FirstName.cs`)
```
FirstName
├─ Id (EF Key)
└─ Name (string, max 50 chars)
```

**LastName Model** (`LastName.cs`)
```
LastName
├─ Id (EF Key)
└─ Name (string, max 50 chars)
```

**College Model** (`College.cs`)
```
College
├─ Id (EF Key)
└─ Name (string, max 100 chars)
```

**Purpose:**
- Seeded from JSON files during database initialization
- Used by PlayerGeneratorService to create realistic players
- Accessed via IPlayerDataRepository (repository pattern)
- ~147 first names, ~100 last names, ~107 colleges

### 2.2 Play Type Models

All play types implement `IPlay` interface and extend concrete Play class.

**IPlay Interface - Universal Properties:**
```csharp
interface IPlay
{
    // Identity
    PlayType PlayType { get; }
    
    // Timing
    int StartTime { get; set; }
    int StopTime { get; set; }
    double ElapsedTime { get; set; }
    
    // Game context
    Possession Possession { get; set; }
    Downs Down { get; set; }
    
    // Players on field
    List<Player> OffensePlayersOnField { get; set; }
    List<Player> DefensePlayersOnField { get; set; }
    
    // Events (can occur on any play)
    List<Penalty> Penalties { get; set; }
    List<Fumble> Fumbles { get; set; }
    List<Injury> Injuries { get; set; }
    
    // Field position & yardage
    int StartFieldPosition { get; set; }
    int EndFieldPosition { get; set; }
    int YardsGained { get; set; }
    
    // Outcomes
    bool PossessionChange { get; set; }
    bool Interception { get; set; }
    bool IsTouchdown { get; set; }
    bool IsSafety { get; set; }
    bool IsFirstDown { get; set; }
    
    // Timing
    bool QuarterExpired { get; set; }
    bool HalfExpired { get; set; }
    bool GameExpired { get; set; }
}
```

**Specific Play Type Models:**

1. **PassPlay** - Forward passes with segments
   ```csharp
   PassPlay : IPlay
   ├─ PassSegments (List<PassSegment>)  // Handles laterals, multiple receivers
   ├─ InterceptionDetails (Interception?)
   ├─ PrimaryPasser (Player?) // First passer
   ├─ FinalReceiver (Player?) // Last receiver
   ├─ IsComplete (bool)
   ├─ TotalAirYards (int)
   └─ IsTwoPointConversion (bool)
   ```

2. **RunPlay** - Running plays with fumble support
   ```csharp
   RunPlay : IPlay
   ├─ RunSegments (List<RunSegment>)  // Handles multiple ball carriers
   └─ InitialBallCarrier (Player?)
   ```

3. **KickoffPlay** - Kickoff plays
   ```csharp
   KickoffPlay : IPlay
   ├─ KickoffSegments (List<ReturnSegment>)
   └─ Returner (Player?)
   ```

4. **PuntPlay** - Punt plays
   ```csharp
   PuntPlay : IPlay
   ├─ PuntSegments (List<ReturnSegment>)
   └─ Returner (Player?)
   ```

5. **FieldGoalPlay** - Field goals and extra points
   ```csharp
   FieldGoalPlay : IPlay
   ├─ KickerSegments (List<ReturnSegment>)
   ├─ IsExtraPointAttempt (bool)
   ├─ IsTwoPointConversion (bool)
   └─ AttemptedYards (int)
   ```

### 2.3 Play Segment Models

**IPlaySegment Interface:**
```csharp
interface IPlaySegment
{
    Player BallCarrier { get; }
    int YardsGained { get; }
    bool EndedInFumble { get; set; }
    Player? FumbledBy { get; set; }
    Player? RecoveredBy { get; set; }
    bool IsOutOfBounds { get; set; }
}
```

**Implementations:**
- `RunSegment` - Ball carrier, yards, fumble details
- `PassSegment` - Passer, receiver, air yards, YAC, completion
- `ReturnSegment` - Returner, yards gained, safety indicators

### 2.4 Related Models

#### **Injury Model** (`Injury.cs`)
```csharp
Injury
├─ Type (InjuryType enum)
│  ├─ None, Ankle, Knee, Shoulder, Concussion, Hamstring
│
├─ Severity (InjurySeverity enum)
│  ├─ Minor (1-2 plays out)
│  ├─ Moderate (rest of drive out)
│  └─ GameEnding (out for game)
│
├─ InjuredPlayer (Player)
├─ PlayNumber (int)
├─ RemovedFromPlay (bool)
├─ ReplacementPlayer (Player?)
└─ PlaysUntilReturn (int)
```

#### **Penalty Model** (`Penalty.cs`)
```csharp
Penalty
├─ Name (PenaltyNames enum - 50+ penalties)
├─ Odds (float)
├─ AwayOdds (float)
├─ CalledOn (Possession)
├─ OccuredWhen (PenaltyOccuredWhen enum)
├─ CommittedBy (Player?)
├─ Yards (int)
└─ Accepted (bool)

PENALTIES INCLUDED:
├─ Offensive: Holding, False Start, Illegal Formation, etc.
├─ Defensive: Pass Interference, Holding, Offside, etc.
├─ Special: Roughing the Passer, Running into Kicker, etc.
└─ 50+ total NFL-accurate penalties with real-world odds
```

#### **Fumble Model** (`Fumble.cs`)
```csharp
Fumble
├─ FumbledBy (Player)
├─ RecoveredBy (Player)
├─ FumbleYards (int)
├─ RecoveryYards (int)
└─ IsSafety (bool)
```

#### **Interception Model** (`Interception.cs`)
```csharp
Interception
├─ InterceptedBy (Player)
├─ InterceptionYards (int)
├─ IsPick6 (bool)
└─ Tackled (bool)
```

#### **Coach Model** (`Coach.cs`)
```csharp
Coach : Person
├─ Role (string)
├─ Age, Experience (years)
├─ Leadership (0-100)
├─ Strategy (0-100)
├─ Motivation (0-100)
├─ Adaptability (0-100)
├─ OffensiveSkill (0-100)
├─ DefensiveSkill (0-100)
├─ SpecialTeamsSkill (0-100)
├─ Reputation (0-100)
├─ ContractYears, Salary
└─ Stats (Dictionary<CoachStatType, int>)
```

### 2.5 Enumerations

```csharp
enum Positions {
    QB, C, G, T, TE, WR, RB, FB,
    DT, DE, LB, OLB, CB, S, FS,
    K, P, LS, H
}

enum Downs {
    First, Second, Third, Fourth, None
}

enum Possession {
    None, Home, Away
}

enum PlayType {
    Kickoff, FieldGoal, Punt, Pass, Run
}
```

---

## 3. DATAACCESSLAYER PROJECT - PERSISTENCE & REPOSITORIES

### 3.1 GridironDbContext Configuration

**Core Entity Mappings:**

```csharp
GridironDbContext : DbContext
│
├─ DbSet<Team> Teams
├─ DbSet<Player> Players
├─ DbSet<Game> Games
├─ DbSet<PlayByPlay> PlayByPlays
│
├─ Player Generation Data
├─ DbSet<FirstName> FirstNames
├─ DbSet<LastName> LastNames
└─ DbSet<College> Colleges
```

**Entity Configuration (OnModelCreating):**

**Teams:**
- Primary Key: Id
- One-to-Many with Players (FK: TeamId)
- DeleteBehavior.SetNull on Team deletion
- Ignores: Stats, Depth Charts, Coaching staff, Scouts (runtime only)

**Players:**
- Primary Key: Id
- Foreign Key: TeamId
- Text columns for strings (FirstName, LastName, College, Height)
- Ignores: CurrentInjury, Stats (not persisted yet)

**Games:**
- Primary Key: Id
- Two Foreign Keys: HomeTeamId, AwayTeamId
- DeleteBehavior.Restrict (don't delete games if team deleted)
- Ignores: Runtime state (CurrentPlay, Plays, Halves, etc.)

**PlayByPlay:**
- Primary Key: Id
- One-to-One with Game
- Large text columns for JSON storage
- CreatedAt timestamp (auto-set to GETUTCDATE())

**FirstName, LastName, College:**
- Primary Keys: Id (auto-generated)
- Required Name field (max length varies: 50/50/100)
- Simple lookup tables for player generation
- Seeded during database initialization

### 3.2 Repository Pattern Implementation

**ITeamRepository Interface:**
```csharp
interface ITeamRepository
{
    Task<List<Team>> GetAllAsync()
    Task<Team?> GetByIdAsync(int teamId)
    Task<Team?> GetByIdWithPlayersAsync(int teamId)  // Includes players
    Task<Team?> GetByCityAndNameAsync(string city, string name)
    Task<Team> AddAsync(Team team)
    Task UpdateAsync(Team team)
    Task DeleteAsync(int teamId)
    Task<int> SaveChangesAsync()
}
```

**IPlayerRepository Interface:**
```csharp
interface IPlayerRepository
{
    Task<List<Player>> GetAllAsync()
    Task<List<Player>> GetByTeamIdAsync(int teamId)
    Task<Player?> GetByIdAsync(int playerId)
    Task<Player> AddAsync(Player player)
    Task UpdateAsync(Player player)
    Task DeleteAsync(int playerId)
}
```

**IGameRepository Interface:**
```csharp
interface IGameRepository
{
    Task<List<Game>> GetAllAsync()
    Task<Game?> GetByIdAsync(int gameId)
    Task<Game?> GetByIdWithTeamsAsync(int gameId)  // Includes teams
    Task<Game> AddAsync(Game game)
    Task UpdateAsync(Game game)
    Task DeleteAsync(int gameId)
}
```

**IPlayerDataRepository Interface:**
```csharp
interface IPlayerDataRepository
{
    Task<List<string>> GetFirstNamesAsync()
    Task<List<string>> GetLastNamesAsync()
    Task<List<string>> GetCollegesAsync()
}
```

**Implementations:**
- `DatabasePlayerDataRepository` - Production (queries Azure SQL tables)
- `JsonPlayerDataRepository` - Testing (loads from JSON files in test project)

**Usage:**
- Injected into `PlayerGeneratorService` for random player generation
- Allows switching between database and JSON sources
- Follows repository pattern for clean separation of concerns

### 3.3 Dependency Injection Setup (Program.cs)

```csharp
// Register DbContext
builder.Services.AddDbContext<GridironDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("GridironDb");
    options.UseSqlServer(connectionString);
});

// Register repositories
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IPlayerDataRepository, DatabasePlayerDataRepository>();

// Register GameManagement services
builder.Services.AddScoped<IPlayerGeneratorService, PlayerGeneratorService>();
builder.Services.AddScoped<ITeamBuilderService, TeamBuilderService>();
builder.Services.AddScoped<IPlayerProgressionService, PlayerProgressionService>();
```

### 3.4 Seed Data System

**TeamsLoader (`TeamsLoader.cs`):**
```csharp
// Load pre-seeded teams from database
public static async Task<Teams> LoadDefaultMatchupAsync()
{
    // Returns Falcons vs Eagles with all players loaded
}
```

**Seed Data Files:**
- `SeedData/SeedDataRunner.cs` - Main orchestrator (run via `dotnet run` in DataAccessLayer)
- `SeedData/PlayerDataSeeder.cs` - Seeds FirstNames, LastNames, Colleges tables from JSON
- `SeedData/TeamSeeder.cs` - Creates Falcons and Eagles teams
- `SeedData/Falcons/` - 9 position-specific seeders
- `SeedData/Eagles/` - 9 position-specific seeders
- Each seeder creates realistic players for that team/position

**Seeding Process:**
1. Run `dotnet run` in DataAccessLayer project
2. Prompts to clear existing data if tables contain data
3. Seeds player generation data (FirstNames, LastNames, Colleges)
4. Seeds teams (Falcons, Eagles)
5. Seeds players for each team by position
6. Displays summary with counts

**Seed Data Sources:**
- Player generation JSON files: `Gridiron.WebApi/SeedData/*.json`
- Contains ~147 first names, ~100 last names, ~107 colleges

---

## 4. EXISTING API CONTROLLERS & SERVICES

### 4.1 REST API Structure (Gridiron.WebApi)

**Technology Stack:**
- ASP.NET Core 8.0
- Swagger/OpenAPI for documentation
- Dependency injection throughout
- Repository pattern for data access

### 4.2 API Controllers

#### **GamesController** (`/api/games`)

```csharp
[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
```

**Endpoints:**

| Method | Endpoint | Purpose |
|--------|----------|---------|
| POST | `/api/games/simulate` | Simulate a new game |
| GET | `/api/games` | Get all simulated games |
| GET | `/api/games/{id}` | Get specific game by ID |
| GET | `/api/games/{id}/plays` | Get play-by-play data |

**Example Request:**
```json
POST /api/games/simulate
{
    "homeTeamId": 1,
    "awayTeamId": 2,
    "randomSeed": 12345
}
```

**Example Response:**
```json
{
    "id": 1,
    "homeTeamId": 1,
    "awayTeamId": 2,
    "homeTeamName": "Atlanta Falcons",
    "awayTeamName": "Philadelphia Eagles",
    "homeScore": 24,
    "awayScore": 21,
    "randomSeed": 12345,
    "isComplete": true,
    "totalPlays": 156
}
```

#### **TeamsController** (planned/stub)
```csharp
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
```

#### **PlayersController** (planned/stub)
```csharp
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
```

### 4.3 Services

#### **IGameSimulationService** (`Services/IGameSimulationService.cs`)

```csharp
interface IGameSimulationService
{
    // Load teams, run simulation, save to database
    Task<Game> SimulateGameAsync(
        int homeTeamId, 
        int awayTeamId, 
        int? randomSeed = null)
    
    Task<Game?> GetGameAsync(int gameId)
    Task<List<Game>> GetGamesAsync()
}
```

#### **GameSimulationService** Implementation

**Key Responsibilities:**
1. Load teams via ITeamRepository
2. Build depth charts using Teams helper
3. Create Game object with loaded teams
4. Run GameFlow.Execute() on background thread
5. Save completed game via IGameRepository
6. Return full game state

**Code Structure:**
```csharp
public async Task<Game> SimulateGameAsync(int homeTeamId, int awayTeamId, int? randomSeed)
{
    // 1. Load teams with players
    var homeTeam = await _teamRepository.GetByIdWithPlayersAsync(homeTeamId);
    var awayTeam = await _teamRepository.GetByIdWithPlayersAsync(awayTeamId);
    
    // 2. Build depth charts (Teams helper)
    var teamsWithDepthCharts = new Teams(homeTeam, awayTeam);
    
    // 3. Create game
    var game = new Game
    {
        HomeTeam = teamsWithDepthCharts.HomeTeam,
        AwayTeam = teamsWithDepthCharts.VisitorTeam,
        HomeTeamId = homeTeamId,
        AwayTeamId = awayTeamId,
        RandomSeed = randomSeed
    };
    
    // 4. Run simulation on background thread
    var rng = randomSeed.HasValue 
        ? new SeedableRandom(randomSeed.Value) 
        : new SeedableRandom();
    
    await Task.Run(() =>
    {
        var gameFlow = new GameFlow(game, rng, _gameLogger);
        gameFlow.Execute();
    });
    
    // 5. Save and return
    await _gameRepository.AddAsync(game);
    return game;
}
```

### 4.4 DTOs (Data Transfer Objects)

Located in `Gridiron.WebApi/DTOs/`:

**GameDto:**
```csharp
public class GameDto
{
    public int Id { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public string HomeTeamName { get; set; }
    public string AwayTeamName { get; set; }
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public int? RandomSeed { get; set; }
    public bool IsComplete { get; set; }
    public int TotalPlays { get; set; }
}
```

**PlayDto, TeamDto, PlayerDto** - Similar structure for plays, teams, and players

---

## 5. GAME SIMULATION LOGIC - STATE LIBRARY

### 5.1 State Machine Architecture

**GameFlow** - The heart of the simulation using Stateless library

```
State Machine with 19 States:

InitializeGame ──StartGameFlow──> PreGame
   ↓                               ↓ (WarmupsCompleted)
                              CoinToss
                                ↓ (CoinTossed)
                              PrePlay ◄──────────────────┐
                          /    |    |    \              │
                         /     |    |     \             │
                   FieldGoal  Run   Kick  Punt ─ PassPlay
                       |       |     |      |      │
                       └───────┴─────┴──────┴──────┘
                              │
                              ↓ (Fumble check)
                          FumbleReturn
                              │
                    ┌─────────┼─────────┐
                    ↓         ↓         ↓
              FieldGoalResult RunResult KickoffResult
              PuntResult      PassPlayResult
                    │         │        │
                    └─────────┼────────┘
                              ↓
                            PostPlay
                              ↓
                    ┌─────────────────┐
                    │ Check if Half   │
                    │ or Game Expired │
                    └────────┬────────┘
                             ↓
                    ┌─────────────────┐
            No      │   Expired?      │    Yes
            ◄───────┤   Quarter?      ├──────────┐
            │       │                 │          │
            │       └────────┬────────┘     QuarterExpired
            │                ↓                  │
            │         PrePlay again             ↓
            │         (Next play)          Check Half?
            │                                │
            │                          Halftime
            │                                │
            └────────────────────────────────┘
            
If game expired → PostGame
```

### 5.2 Game States (19 Total)

| State | Purpose | Transitions |
|-------|---------|-------------|
| InitializeGame | Setup | → PreGame |
| PreGame | Warmups | → CoinToss |
| CoinToss | Determine possession | → PrePlay |
| PrePlay | Call play, check pre-snap penalties | → PlayType states or PostPlay |
| FieldGoal | Execute FG attempt | → FumbleReturn |
| RunPlay | Execute run | → FumbleReturn |
| Kickoff | Execute kickoff | → FumbleReturn |
| Punt | Execute punt | → FumbleReturn |
| PassPlay | Execute pass | → FumbleReturn |
| FumbleReturn | Check for fumbles | → Result states |
| FieldGoalResult | Process FG result | → PostPlay |
| RunPlayResult | Process run result | → PostPlay |
| KickoffResult | Process kickoff result | → PostPlay |
| PuntResult | Process punt result | → PostPlay |
| PassPlayResult | Process pass result | → PostPlay |
| PostPlay | Handle scoring, downs, turnovers | → PrePlay or special states |
| Halftime | Half break | → PrePlay |
| QuarterExpired | Quarter timeout | → QuarterExpired or Halftime |
| PostGame | Game end | (terminal) |

### 5.3 Play Execution Classes

Located in `StateLibrary/Plays/`:

**Run.cs** - Execute running plays
**Pass.cs** - Execute passing plays
**Kickoff.cs** - Execute kickoff plays
**Punt.cs** - Execute punt plays
**FieldGoal.cs** - Execute field goal attempts

**Execution Flow for Each Play:**
1. Call `Execute(game)` method
2. Perform pre-play setup (select players)
3. Run skill checks (25+ checks based on play type)
4. Calculate outcomes (yards, completion, fumble, injury, etc.)
5. Update game state
6. Record play details

### 5.4 Skill Checks (25+ Statistical Checks)

Located in `StateLibrary/SkillsChecks/`:

**Checks Performed per Play Type:**

**RUN PLAYS:**
- FumbleOccurred - Does ball carrier fumble?
- TackleBreak - How many yards after first contact?
- InjuryOccurred - Does anyone get hurt?
- BigRun - Does it gain 20+ yards?

**PASS PLAYS:**
- PassCompletion - QB throws accurate pass?
- PassProtection - Does OL hold blocks?
- InterceptionOccurred - Does defense pick it off?
- YardsAfterCatch - Receiver gains extra yards?
- QBPressure - Sack/pressure on QB?
- CoveragePenalty - PI/holding on coverage?

**KICKOFFS:**
- KickoffDistance - How far was kickoff?
- OnSideKick - Special kick?
- KickoffReturnYards - Returner gains how much?
- FairCatch - Returner calls fair catch?

**PUNTS:**
- PuntDistance - How far was punt?
- PuntHangTime - How long was ball in air?
- PuntDownedOccurred - Kick downed in field?
- PuntBlockOccurred - Was punt blocked?
- PuntReturnYards - Returner gains how much?

**FIELD GOALS:**
- FieldGoalMakeOccurred - Does kicker make it?
- FieldGoalBlockOccurred - Is FG blocked?

**PENALTIES:**
- PreSnapPenalty - Offense commits before snap?
- BlockingPenalty - Blocks illegal?
- CoveragePenalty - Defense commits illegal act?
- PostPlayPenalty - Any post-play infractions?

### 5.5 Statistical Models

**InjuryProbabilities.cs:**
- Position-specific injury risks
- Severity weightings
- Play-type modifiers (QB sack = 2x, kickoff = 1.67x)
- Gang tackle multiplier (1.4x)
- Big play multiplier (1.2x for 20+ yard plays)

**GameProbabilities.cs:**
- Base probabilities for all events
- Difficulty modifiers
- Player attribute scaling

### 5.6 Injury System Integration

**How Injuries Work:**

1. **During Play Execution:**
   - After each play segment, injury skill check runs
   - Ball carrier checked
   - Up to 2 tacklers checked
   - Injury probability based on fragility + play-type multipliers

2. **Injury Types:**
   - Ankle, Knee, Shoulder, Concussion, Hamstring
   - Each has 0-100 position-specific risk factor
   - (Example: Ankle = high for RB, low for QB)

3. **Severity Levels:**
   - **Minor:** Out for 1-2 plays (recovery: recovery checks after each play)
   - **Moderate:** Out for rest of current drive
   - **Game-Ending:** Out for game remainder

4. **Player Substitution:**
   - Depth chart used to select replacement from same position
   - If no backup available, field without full roster
   - Game continues with reduced staff if necessary

5. **Recovery Tracking:**
   - PlayNumber recorded when injured
   - PlaysUntilReturn countdown per play
   - Automatic substitution back in when recovered

---

## 6. EXISTING TEST PROJECTS

### 6.1 Test Framework

**Framework:** MSTest (Microsoft.VisualStudio.TestTools)
**Coverage Tool:** Coverlet
**Status:** 839 passing tests (100% pass rate)

### 6.2 UnitTestProject1 (Main Test Suite)

**44 Test Files, 839 Tests**

**Test Categories:**

| Category | File | Tests | Focus |
|----------|------|-------|-------|
| **Play Execution** | PassPlayExecutionTests.cs | ~80 | Pass play outcomes |
| | RunPlayExecutionTests.cs | ~75 | Run play outcomes |
| | KickoffPlayExecutionTests.cs | ~50 | Kickoff plays |
| | PuntPlayExecutionTests.cs | ~50 | Punt plays |
| | FieldGoalPlayExecutionTests.cs | ~50 | Field goal attempts |
| **Skills Checks** | SkillsChecksTests.cs | ~100 | All skill check logic |
| | PassPlaySkillsChecksTests.cs | ~50 | Pass-specific checks |
| | RunPlaySkillsChecksTests.cs | ~50 | Run-specific checks |
| **Penalties** | PenaltyEnforcementRulesTests.cs | ~80 | Penalty rules |
| | PenaltyAcceptanceTests.cs | ~60 | Acceptance logic |
| | PlayerDisciplinePenaltyTests.cs | ~40 | Discipline effects |
| | DeadBallPenaltyTests.cs | ~40 | Dead ball rules |
| **Injuries** | InjurySystemTests.cs | ~100 | Injury tracking |
| **Scoring** | ScoringTests.cs | ~60 | Touchdown/FG scoring |
| | TwoPointConversionTests.cs | ~50 | 2-point logic |
| **Game Flow** | ... (various integration tests) | ~200 | Full game flow |

### 6.3 Test Infrastructure (Helpers)

**Located in `UnitTestProject1/Helpers/`:**

**TestGame.cs:**
```csharp
// Create game with specific initial conditions
var game = new TestGame()
    .WithQuarter(Quarter.First)
    .WithDown(Downs.First)
    .WithFieldPosition(20)
    .WithYardsToGo(10)
    .Build();
```

**TestTeams.cs:**
```csharp
// Load pre-built test teams (Falcons vs Eagles)
var teams = TestTeams.CreateTestTeams();
var homeTeam = teams.HomeTeam;  // Atlanta Falcons
var awayTeam = teams.VisitorTeam;  // Philadelphia Eagles
```

**TestFluentSeedableRandom.cs:**
```csharp
// Builder pattern for reproducible random scenarios
var rng = new TestFluentSeedableRandom()
    .Skill(70)           // Skill check passes at 70%
    .Distance(15)        // Returns 15 yards
    .Completion(true)    // Marks pass as complete
    .Build();
```

**Scenario Builders:**
- `PassPlayScenarios.cs` - Pre-built pass play scenarios
- `RunPlayScenarios.cs` - Pre-built run play scenarios
- `SkillCheckScenarios.cs` - Pre-built skill check scenarios
- `PenaltyScenarios.cs` - Pre-built penalty scenarios

### 6.4 Gridiron.WebApi.Tests

**Tests for API Services**
```csharp
GameSimulationServiceTests
├─ SimulateGame_WhenBothTeamsExist_LoadsTeamsWithPlayers
├─ SimulateGame_WithRandomSeed_ProducesDeterministicResults
├─ SimulateGame_InvalidTeamId_ThrowsException
└─ GetGame_WithValidId_ReturnsGameWithTeams

TeamsControllerTests (stubs)
PlayersControllerTests (stubs)
```

Uses Moq for repository mocking:
```csharp
var mockTeamRepository = new Mock<ITeamRepository>();
mockTeamRepository
    .Setup(r => r.GetByIdWithPlayersAsync(1))
    .ReturnsAsync(testTeam);
```

### 6.5 GameManagement.Tests

**Comprehensive tests for player/team generation and management services:**

**PlayerGeneratorServiceTests (35+ tests):**
- Random player generation across all positions
- Draft class generation with rookies
- Position-specific attribute ranges (QB passing, RB rushing, etc.)
- Jersey number assignment by position
- Height/weight generation by position
- Reproducible generation with seeds

**TeamBuilderServiceTests (31 tests):**
- Team creation with 53-player rosters
- Depth chart assignment (8 depth charts per team)
- Roster validation (min/max players, position requirements)
- Player assignment and removal
- Coaching staff and support staff setup

**PlayerProgressionServiceTests (40 tests):**
- Age curve progression (22-26 development, 27-30 peak, 31-34 decline, 35+ rapid decline)
- Retirement logic based on age and overall rating
- Overall rating calculation from player attributes
- Salary calculation based on position and overall rating
- Attribute adjustments based on age and potential

**Test Infrastructure:**
- Uses `JsonPlayerDataRepository` to load test data from JSON files
- Test data located in `GameManagement.Tests/TestData/`
- xUnit framework with Moq and FluentAssertions
- All tests passing with comprehensive coverage

---

## 7. ARCHITECTURAL PATTERNS & CONVENTIONS

### 7.1 Design Patterns Used

| Pattern | Where Used | Purpose |
|---------|-----------|---------|
| **State Machine** | StateLibrary/GameFlow.cs | Game flow control |
| **Repository** | DataAccessLayer | Data access abstraction |
| **Dependency Injection** | All projects | Service composition |
| **Builder** | Test helpers | Fluent test setup |
| **Strategy** | SkillsChecks | Pluggable skill checks |
| **Template Method** | BaseClasses | Common skill check logic |
| **Factory** | PlayerGenerator | Object creation |

### 7.2 Code Organization Conventions

**Project Structure:**
- Models in root or domain folder
- Interfaces separated from implementations
- Tests mirror project structure
- Helpers in dedicated folder
- Service classes in Services folder

**Naming Conventions:**
- Interfaces: `I{Name}` (ITeamRepository, IPlay)
- Enums: PascalCase (Positions, Downs, Possession)
- Classes: PascalCase (Player, Team, GameFlow)
- Properties: PascalCase with auto-properties
- Private fields: `_fieldName`
- Constants: ALL_CAPS (where used)

**Method Naming:**
- Async methods: `{Action}Async` (SimulateGameAsync, GetByIdAsync)
- Getters: `Get{Entity}` or `Get{Entity}ByX`
- Builders: fluent pattern with chaining

### 7.3 Architectural Rules & Constraints

**RULE 1: Database Access Only in DataAccessLayer**
- ❌ Never reference GridironDbContext outside DataAccessLayer
- ❌ Never use EF Core methods in WebApi, Console, or GameManagement
- ✅ Always inject ITeamRepository, IPlayerRepository, IGameRepository

**RULE 2: Play Execution Through IPlay**
- ✅ All play logic implements IPlay interface
- ✅ StateLibrary orchestrates play execution
- ✅ Plays are stateless, mutation occurs on Game object

**RULE 3: Skill Checks Are Probabilistic**
- ✅ All probability-based checks use ISeedableRandom
- ✅ Seeding allows reproducible simulations
- ✅ Tests use deterministic RNG builders

**RULE 4: Logging Through ILogger**
- ✅ DomainObjects use ILogger for play-by-play output
- ✅ Game.Logger provides context
- ✅ ServiceCollection configures logging providers

**RULE 5: Depth Charts Required**
- ✅ Teams helper builds depth charts from player rosters
- ✅ Simulatio requires depth charts to select players
- ✅ Multiple depth charts for different play situations

### 7.4 Data Flow Patterns

**Game Simulation Flow:**
```
SimulateGameAsync(homeTeamId, awayTeamId, seed)
  └─ Load teams via ITeamRepository
  └─ Build depth charts via Teams helper
  └─ Create Game object
  └─ Create SeedableRandom(seed)
  └─ Create GameFlow(game, rng, logger)
  └─ GameFlow.Execute()
       └─ State transitions
       └─ PrePlay action: select plays
       └─ Play action: call Play.Execute(game)
            └─ Skill checks via ISeedableRandom
            └─ Update game state
            └─ Record outcomes
       └─ PostPlay action: handle scoring, turnovers
  └─ Save game via IGameRepository
  └─ Return completed game
```

**Play Execution Pattern:**
```
Play.Execute(game)
  └─ Setup: Select players from depth charts
  └─ Pre-play: Skill checks (10+ checks)
  └─ Execution: Run segments, calculate yardage
  └─ Injuries: Check for injuries on ball carrier + tacklers
  └─ Penalties: Check for offensive/defensive fouls
  └─ Post-play: Update game field position, downs
  └─ Return: All details stored on game.CurrentPlay
```

### 7.5 Testing Patterns

**Test Structure:**
```csharp
[TestClass]
public class SomeTests
{
    private DomainObjects.Helpers.Teams _teams;
    private TestGame _testGame;
    
    [TestInitialize]
    public void Setup()
    {
        _teams = TestTeams.CreateTestTeams();
        _testGame = new TestGame();
    }
    
    [TestMethod]
    public void Scenario_Condition_ExpectedOutcome()
    {
        // Arrange
        var game = CreateGameWithSpecificState();
        var rng = PlayPlayScenarios.SpecificScenario();
        
        // Act
        var play = new Pass(rng);
        play.Execute(game);
        
        // Assert
        Assert.AreEqual(expected, game.CurrentPlay.YardsGained);
    }
}
```

**Deterministic Testing:**
- TestFluentSeedableRandom for controlled outcomes
- Scenario builders for complex setups
- Exact outcome verification
- No flaky tests

### 7.6 Documentation Patterns

**Class Documentation:**
```csharp
/// <summary>
/// Brief description of what class does
/// </summary>
public class MyClass
{
    /// <summary>
    /// What property represents
    /// </summary>
    public int MyProperty { get; set; }
    
    /// <summary>
    /// What method does and what it returns
    /// </summary>
    /// <param name="param">What this parameter does</param>
    /// <returns>What method returns</returns>
    public async Task<Result> DoSomethingAsync(string param)
    { }
}
```

**Code Organization:**
```csharp
public class MyClass
{
    // ========================================
    // FIELDS
    // ========================================
    private readonly IService _service;
    
    // ========================================
    // CONSTRUCTORS
    // ========================================
    public MyClass(IService service) { }
    
    // ========================================
    // PUBLIC METHODS
    // ========================================
    public void DoSomething() { }
    
    // ========================================
    // PRIVATE METHODS
    // ========================================
    private void HelperMethod() { }
}
```

---

## SUMMARY: KEY TAKEAWAYS FOR GAMEMANAGEMENT IMPLEMENTATION

### What's Already Built:
1. ✅ Complete game simulation engine (StateLibrary)
2. ✅ All 5 play types fully working
3. ✅ 25+ skill checks
4. ✅ 50+ penalties
5. ✅ Injury system (position-specific, severity levels)
6. ✅ Database persistence layer
7. ✅ REST API skeleton
8. ✅ 839 passing tests

### What GameManagement Has Implemented:
1. ✅ Player generation service (fully implemented with repository pattern)
   - Random player generation across all positions
   - Draft class generation with configurable rounds
   - Position-specific attribute ranges and physical characteristics
   - Uses IPlayerDataRepository for database/JSON flexibility
2. ✅ Team building service (fully implemented)
   - Team creation with 53-player roster management
   - 8 depth chart assignments (offense, defense, special teams)
   - Roster validation with position requirements
   - Coaching and support staff setup
3. ✅ Player progression system (fully implemented)
   - Age curve progression (development, peak, decline phases)
   - Retirement logic based on age and performance
   - Overall rating calculation
   - Salary calculation by position and rating
4. ✅ Comprehensive test coverage (100+ tests passing)
   - PlayerGeneratorServiceTests (35+ tests)
   - TeamBuilderServiceTests (31 tests)
   - PlayerProgressionServiceTests (40 tests)

### What GameManagement Still Needs:
1. ⚠️ Coaching AI (coaches exist, no decision-making logic)
2. ⚠️ Scouting system (scouts exist, no evaluation/grading)
3. ⚠️ Draft mechanics (player generation exists, need draft UI/logic)
4. ⚠️ Season management (games work, need season structure)
5. ⚠️ Team morale/chemistry integration with game outcomes
6. ⚠️ Contract negotiation and salary cap management
7. ⚠️ Free agency system
8. ⚠️ Trade logic and validation

### Architecture Constraints to Remember:
- **Only DataAccessLayer talks to database**
- **Inject repositories, never DbContext**
- **Use ISeedableRandom for determinism**
- **Leverage depth charts for player selection**
- **Add logging via ILogger**
- **Use DI for all services**
- **Write tests as you go**

### Key Classes to Know:
- `Game` - Main game state container
- `Team` - Team data with players and coaching staff
- `Player` - Individual player with 25+ attributes
- `GameFlow` - State machine orchestrator
- `IPlay` - Interface all plays implement
- `ITeamRepository` - How you access teams from DB
- `IPlayerRepository` - How you access players from DB
- `IGameRepository` - How you access games from DB
- `IPlayerDataRepository` - How you access player generation data (names, colleges)
- `GameSimulationService` - Orchestrates simulation flow
- `PlayerGeneratorService` - Generates random players and draft classes
- `TeamBuilderService` - Creates and manages team rosters
- `PlayerProgressionService` - Handles aging and retirement

---

## 12. CI/CD PIPELINE & TESTING STRATEGY

### 12.1 GitHub Actions Workflow

**File:** `.github/workflows/frontend-tests.yml`

Automated testing runs on every pull request that modifies frontend code or the workflow file.

**Triggers:**
- Pull requests to `master` branch
- Pushes to `master` branch
- Manual workflow dispatch
- Only when frontend files change (`gridiron-web/**`)

### 12.2 Testing Pipeline Architecture

```
┌──────────────────────────────────────────────────────────────┐
│         PHASE 1: Component & Integration Tests               │
│                    (~2-3 minutes)                            │
│                                                              │
│  Environment: Ubuntu Latest, Node.js 20                     │
│  Dependencies: npm ci                                        │
│  Command: npm test -- --run                                 │
│                                                              │
│  Tests Run:                                                  │
│    ✓ 15 component tests (Vitest + React Testing Library)   │
│    ✓ MSW mocks for API integration testing                 │
│    ✓ No backend/database required                          │
│                                                              │
│  Artifacts: Test coverage reports                           │
│  Retention: 7 days                                          │
└──────────────────────────────────────────────────────────────┘
          ↓ (Only if Phase 1 passes)
┌──────────────────────────────────────────────────────────────┐
│           PHASE 2: End-to-End Tests                          │
│                    (~5-8 minutes)                            │
│                                                              │
│  Infrastructure Setup:                                       │
│    1. SQL Server 2022 Container                             │
│       - Image: mcr.microsoft.com/mssql/server:2022-latest  │
│       - Port: 1433                                          │
│       - Health checks with sqlcmd                           │
│       - Password: YourStrong!Passw0rd                       │
│                                                              │
│    2. .NET 8 SDK Setup                                      │
│       - Restore dependencies                                │
│       - Install dotnet-ef 8.0                               │
│       - Build Gridiron.WebApi                               │
│                                                              │
│    3. Database Migration                                    │
│       - Command: dotnet ef database update                  │
│       - Target: GridironCI database                         │
│                                                              │
│    4. Database Seeding (Non-Interactive)                    │
│       - Command: dotnet run -- --seed --force               │
│       - Seeds: 2 teams, 106 players                         │
│       - Player data: 147 first names, 126 last names,      │
│                      107 colleges                           │
│                                                              │
│    5. API Startup                                           │
│       - Run in background                                   │
│       - Port: 5000                                          │
│       - Verify with: curl /swagger/index.html              │
│       - Verify data: curl /api/teams                        │
│                                                              │
│    6. Frontend Setup                                        │
│       - Install Playwright browsers (chromium)              │
│       - Install npm dependencies                            │
│       - Vite dev server auto-starts                         │
│                                                              │
│  Tests Run:                                                  │
│    ✓ 10 Playwright E2E tests                                │
│    ✓ Tests against real API with real database             │
│    ✓ Full integration coverage                             │
│                                                              │
│  Tests Covered:                                              │
│    - Homepage rendering and navigation                      │
│    - Teams page data loading                                │
│    - Game simulation flow                                   │
│    - Navigation between pages                               │
│                                                              │
│  Artifacts:                                                  │
│    - Playwright HTML report (always)                        │
│    - Test traces (on failure only)                          │
│  Retention: 7 days                                          │
└──────────────────────────────────────────────────────────────┘
```

### 12.3 Test Coverage Summary

**Backend Tests:**
- **Unit Tests:** 839 MSTest tests (100% pass rate)
  - Domain object tests
  - State machine logic tests
  - Play execution tests
  - Penalty system tests
  - Injury system tests

- **Integration Tests:** Gridiron.IntegrationTests
  - API endpoint tests
  - Soft delete cascade tests
  - Database operation tests

**Frontend Tests:**
- **Component Tests:** 15 tests (Vitest + React Testing Library)
  - Navigation component
  - Loading states
  - Error states
  - Team components
  - Homepage components

- **Integration Tests:** 15 tests (MSW)
  - API mocking
  - Data fetching flows
  - Error handling
  - Loading states

- **E2E Tests:** 10 tests (Playwright)
  - Homepage functionality
  - Teams page
  - Game simulation
  - Navigation flows

**Total Test Count:** 869 automated tests

### 12.4 Seeding Strategy for CI/CD

**Challenge:** Database seeding requires interactive confirmation

**Solution:** Non-interactive `--force` flag

**Implementation:**
```csharp
// DataAccessLayer/SeedData/SeedDataRunner.cs
public static async Task RunAsync(string[] args)
{
    bool forceMode = args.Contains("--force", StringComparer.OrdinalIgnoreCase);

    if (existingData && !forceMode)
    {
        // Interactive mode: Prompt user
        Console.Write("Clear existing data? (y/n): ");
        var response = Console.ReadLine();
    }
    else if (existingData && forceMode)
    {
        // CI/CD mode: Auto-clear
        Console.WriteLine("Force mode - clearing automatically...");
    }
}
```

**Environment Variable Strategy:**
- Seeding: `ConnectionStrings__DefaultConnection`
- API Runtime: `ConnectionStrings__GridironDb`
- Both point to same database: `GridironCI`

### 12.5 Local Testing Guide

**Component Tests:**
```bash
cd gridiron-web
npm test              # Watch mode
npm test -- --run     # Run once
npm run test:ui       # Visual test runner
```

**E2E Tests (Requires API):**
```bash
# Terminal 1: Start API
cd Gridiron.WebApi
dotnet run

# Terminal 2: Run E2E tests
cd gridiron-web
npm run test:e2e           # Headless
npm run test:e2e:ui        # With UI
npx playwright test --debug # Debug mode
```

**Backend Tests:**
```bash
dotnet test                 # All tests
dotnet test --filter "FullyQualifiedName~GameFlow"  # Specific tests
```

### 12.6 Key Testing Documentation

- **Frontend:** `gridiron-web/TESTING.md` - Complete frontend testing guide
- **Backend:** `API_TESTING_GUIDE.md` - Backend API testing guide
- **Workflow:** `.github/workflows/frontend-tests.yml` - CI/CD configuration

---

