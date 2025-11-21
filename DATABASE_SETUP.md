# Gridiron Database Setup Guide

## Project Architecture

The persistence layer has been properly separated into distinct projects following clean architecture principles:

```
/gridiron
├── DomainObjects/          - Pure domain models (POCOs - no EF dependencies)
│   ├── Team.cs
│   ├── Player.cs
│   ├── Game.cs
│   └── PlayByPlay.cs
├── DataAccessLayer/        - EF Core persistence layer
│   ├── GridironDbContext.cs
│   ├── GridironDbContextFactory.cs
│   └── appsettings.json    - Connection string configuration
├── StateLibrary/           - Game simulation engine
├── GridironConsole/        - Console application
└── UnitTestProject1/       - Tests
```

**Key Separation:**
- **DomainObjects** - Contains only domain logic, no database dependencies
- **DataAccessLayer** - All Entity Framework Core code and configuration
- **GridironConsole** - References both DomainObjects and DataAccessLayer

---

## What Has Been Configured

### 1. **Domain Objects Enhanced** ✅
- Added `Id` properties to `Team`, `Player`, and `Game` entities (primary keys)
- Added `TeamId` foreign key to `Player` (nullable - for free agents)
- Added `HomeTeamId` and `AwayTeamId` foreign keys to `Game`
- Added `RandomSeed` property to `Game` for reproducible simulations
- Created new `PlayByPlay` entity to store game execution logs
- **No EF dependencies in DomainObjects** - keeps models clean

### 2. **DataAccessLayer Project Created** ✅
- New class library project: `/DataAccessLayer/`
- Contains all Entity Framework Core code
- References DomainObjects project
- `GridironDbContext.cs` - DbContext with entity configurations
- `GridironDbContextFactory.cs` - Design-time factory for migrations
- `appsettings.json` - Connection string template

### 3. **Entity Configurations** ✅
Configured in `GridironDbContext`:
- **Team → Players** (one-to-many, SetNull on delete)
- **Game → HomeTeam, AwayTeam** (many-to-one, Restrict delete)
- **Game → PlayByPlay** (one-to-one, Cascade delete)
- Ignored complex runtime properties (depth charts, coaches, stat dictionaries)

### 4. **NuGet Packages** ✅
**DataAccessLayer project:**
- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Microsoft.EntityFrameworkCore.Design 8.0.0
- Microsoft.Extensions.Configuration packages

**DomainObjects project:**
- Clean - only Newtonsoft.Json and Logging.Abstractions

### 5. **Configuration Setup** ✅
- User Secrets ID: `gridiron-football-sim-2024`
- `appsettings.json` template in DataAccessLayer
- GridironConsole references DataAccessLayer

---

## Next Steps: Running Migrations

### Step 1: Set Your Azure SQL Connection String

Run these commands in your **local terminal** where you have .NET SDK installed:

```bash
cd /path/to/gridiron/DataAccessLayer

# Set the connection string in user secrets
dotnet user-secrets set "ConnectionStrings:GridironDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Replace placeholders with your Azure SQL Database credentials:**
- `YOUR_SERVER` - Azure SQL server name (e.g., `gridiron-dev`)
- `YOUR_DATABASE` - Database name (e.g., `gridiron`)
- `YOUR_USERNAME` - SQL admin username
- `YOUR_PASSWORD` - SQL admin password

**Alternative Options:**
- **appsettings.json** - Edit `/DataAccessLayer/appsettings.json` (less secure, don't commit)
- **Environment Variable** - Set `ConnectionStrings__GridironDb`

---

### Step 2: Create the Initial Migration

From the **solution root directory** (`C:\projects\gridiron`):

```bash
cd C:\projects\gridiron

# Create the initial migration
dotnet ef migrations add InitialCreate --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
```

**Important:**
- Use **forward slashes** (`/`) not backslashes (`\`)
- Include the `.csproj` file name
- Run from the solution root, not the DataAccessLayer directory

This will:
- Analyze your `GridironDbContext` and entity models
- Generate C# migration files in `/DataAccessLayer/Migrations/`
- Create SQL to build tables: `Teams`, `Players`, `Games`, `PlayByPlays`

**Expected Output:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

---

### Step 3: Apply Migration to Azure SQL Database

```bash
# From solution root
dotnet ef database update --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
```

This will:
- Connect to your Azure SQL Database
- Execute the migration SQL
- Create all tables with proper relationships

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20250119_InitialCreate'.
Done.
```

---

### Step 4: Verify Tables in VS Code

1. **Open VS Code SQL Server extension**
2. **Connect to your Azure SQL Database**
3. **Expand Tables** - you should see:
   - `dbo.Teams`
   - `dbo.Players`
   - `dbo.Games`
   - `dbo.PlayByPlays`
   - `dbo.__EFMigrationsHistory` (tracks applied migrations)

4. **Inspect a table** - right-click `Teams` → "Select Top 1000"
   - Should be empty but structure should exist

---

## Database Schema

### Teams Table
```sql
- Id (int, PK, Identity)
- Name (nvarchar(100), required)
- City (nvarchar(100))
- Budget (int)
- Championships (int)
- Wins (int)
- Losses (int)
- Ties (int)
- FanSupport (int)
- Chemistry (int)
```

### Players Table
```sql
- Id (int, PK, Identity)
- TeamId (int, FK to Teams, nullable)
- FirstName (nvarchar(50))
- LastName (nvarchar(50), required)
- Position (int - enum)
- Number (int)
- Height (nvarchar(10))
- Weight (int)
- Age (int)
- Exp (int)
- College (nvarchar(100))
- [All attributes: Speed, Strength, Passing, Catching, etc.]
- Salary (int)
- ContractYears (int)
- Health (int)
- IsRetired (bit)
- Potential (int)
- Progression (int)
```

### Games Table
```sql
- Id (int, PK, Identity)
- HomeTeamId (int, FK to Teams, required)
- AwayTeamId (int, FK to Teams, required)
- RandomSeed (int, nullable)
- WonCoinToss (int - enum)
- DeferredPossession (bit)
- FieldPosition (int)
- YardsToGo (int)
- CurrentDown (int - enum)
- HomeScore (int)
- AwayScore (int)
```

### PlayByPlays Table
```sql
- Id (int, PK, Identity)
- GameId (int, FK to Games, unique, required)
- PlaysJson (nvarchar(max))
- PlayByPlayLog (nvarchar(max))
- CreatedAt (datetime2, default UTC now)
```

---

## Using the Database in Your Code

### Example: Basic CRUD Operations

```csharp
using DataAccessLayer;
using DomainObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<GridironDbContext>()
    .Build();

// Create DbContext
var options = new DbContextOptionsBuilder<GridironDbContext>()
    .UseSqlServer(configuration.GetConnectionString("GridironDb"))
    .Options;

using var db = new GridironDbContext(options);

// Create a team
var chiefs = new Team
{
    Name = "Chiefs",
    City = "Kansas City",
    Budget = 200000000,
    Players = new List<Player>()
};
db.Teams.Add(chiefs);
await db.SaveChangesAsync();

// Query teams with players
var teams = await db.Teams
    .Include(t => t.Players)
    .ToListAsync();
```

### Example: Save Game Result with Play-by-Play

```csharp
using Newtonsoft.Json;

// After game simulation
game.RandomSeed = seedValue;  // Store seed for recreation
db.Games.Add(game);
await db.SaveChangesAsync();

// Save play-by-play
var playByPlay = new PlayByPlay
{
    GameId = game.Id,
    PlaysJson = JsonConvert.SerializeObject(game.Plays),
    PlayByPlayLog = capturedLogOutput,  // From logger
    CreatedAt = DateTime.UtcNow
};
db.PlayByPlays.Add(playByPlay);
await db.SaveChangesAsync();
```

### Example: Recreate Exact Game

```csharp
// Load saved game
var savedGame = await db.Games
    .Include(g => g.HomeTeam).ThenInclude(t => t.Players)
    .Include(g => g.AwayTeam).ThenInclude(t => t.Players)
    .FirstAsync(g => g.Id == gameId);

// Use same seed = identical results every time
var rng = new SeedableRandom(savedGame.RandomSeed.Value);
var gameFlow = new GameFlow(savedGame, rng, logger);
gameFlow.Execute();  // Exact same plays!
```

---

## Troubleshooting

### "Connection string not found"
- Verify you ran `dotnet user-secrets set` in **DataAccessLayer** directory
- Check that User Secrets ID matches: `gridiron-football-sim-2024`
- Or edit `/DataAccessLayer/appsettings.json` with real connection string

### "Login failed for user"
- Verify SQL Authentication credentials in connection string
- Confirm user has `db_owner` role or create table permissions
- Check Azure SQL authentication mode (should allow SQL auth)

### "Cannot connect to server"
- Verify Azure SQL firewall rules include your IP address
- Test connection using VS Code SQL Server extension first
- Check server name format: `yourserver.database.windows.net`

### "Build failed" during migration
- Run `dotnet restore` in solution root directory
- Verify .NET 8 SDK is installed: `dotnet --version`
- Check for compilation errors: `dotnet build`

### "Unable to retrieve project metadata" or "Project file does not exist"
- Ensure you're running from the **solution root directory** (`C:\projects\gridiron`)
- Use **forward slashes** (`/`) in paths, not backslashes (`\`)
- Include the full `.csproj` file name in the path
- Correct format:
  ```bash
  dotnet ef migrations add MigrationName --project DataAccessLayer/DataAccessLayer.csproj --startup-project Gridiron.WebApi/Gridiron.WebApi.csproj
  ```

### "No executable found matching command 'dotnet-ef'"
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

---

## Project Structure Benefits

**Clean Separation of Concerns:**
- ✅ Domain models remain pure POCOs
- ✅ EF Core isolated in DataAccessLayer
- ✅ Easy to swap out persistence later (e.g., different ORM)
- ✅ Domain layer has no infrastructure dependencies
- ✅ Follows SOLID principles

**Testing Benefits:**
- Domain objects can be tested without database
- DataAccessLayer can be mocked for unit tests
- Integration tests can target DataAccessLayer specifically

**Future Extensibility:**
- Add repositories in DataAccessLayer
- Add domain services without touching persistence
- Migrations isolated in one project
- Easy to add additional data stores (NoSQL for user profiles, etc.)

---

## What's Next?

Once your tables are created and verified:

1. **Test basic CRUD** - Insert a team and player, query them back
2. **Implement game saving** - Store game results after simulation
3. **Build repositories** (optional) - Wrap DbContext in repository pattern
4. **Add data seeding** - Create initial teams/players on startup
5. **Implement statistics** - Start populating player stats during games

Let me know when you've completed the migrations and we can test saving/loading games!
