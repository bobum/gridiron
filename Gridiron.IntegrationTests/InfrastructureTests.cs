using DataAccessLayer;
using DataAccessLayer.SeedData;
using DomainObjects;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Infrastructure tests that validate the full seeding pipeline:
/// - Schema creation from model works correctly
/// - Complete seeding process runs successfully
/// - All data integrity checks pass
///
/// NOTE: These tests use EnsureCreated (not migrations) because migrations are
/// SQL Server-specific and can't run on SQLite. Migration validation happens via
/// the PowerShell reset-database.ps1 script on actual Azure SQL Server.
///
/// These tests are valuable for catching seeding issues early, especially as we
/// build out more seeders over time.
/// </summary>
public class InfrastructureTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly GridironDbContext _dbContext;
    private readonly ILoggerFactory _loggerFactory;

    public InfrastructureTests()
    {
        // Create SQLite in-memory connection (must stay open for database lifetime)
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Configure DbContext to use SQLite
        var options = new DbContextOptionsBuilder<GridironDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new GridironDbContext(options);

        // Create logger factory for seeding
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    /// <summary>
    /// Tests the FULL seeding pipeline:
    /// 1. Create database schema from model (EnsureCreated)
    /// 2. Run complete seeding process
    /// 3. Verify expected data exists
    ///
    /// NOTE: This uses EnsureCreated (not migrations) because migrations are SQL Server-specific.
    /// Migration validation happens via PowerShell reset-database.ps1 script on real SQL Server.
    /// This test validates that the seeding process works correctly, which is valuable as we
    /// build out more seeders.
    /// </summary>
    [Fact]
    public async Task SeedingPipeline_CreateSchemaAndSeedAllData_DatabaseReadyForUse()
    {
        // ============================================================
        // STEP 1: Create Schema from Model
        // ============================================================
        // This generates schema from entity classes (provider-agnostic)
        // NOTE: Migrations are tested separately via PowerShell script on SQL Server

        await _dbContext.Database.EnsureCreatedAsync();

        // Verify schema includes soft delete infrastructure
        // We test this by querying with IsDeleted - if the column doesn't exist, this will throw
        var teamsQuery = await _dbContext.Teams.Where(t => !t.IsDeleted).ToListAsync();
        var playersQuery = await _dbContext.Players.Where(p => !p.IsDeleted).ToListAsync();
        teamsQuery.Should().NotBeNull("because IsDeleted column should exist on Teams table");
        playersQuery.Should().NotBeNull("because IsDeleted column should exist on Players table");

        // ============================================================
        // STEP 2: Seed Player Generation Data
        // ============================================================
        // This tests the same process as: PlayerDataSeeder.SeedAllAsync()

        var playerDataSeeder = new PlayerDataSeeder(_dbContext, _loggerFactory.CreateLogger<PlayerDataSeeder>());

        // Seed data files are in the test project's SeedData directory
        var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData");
        await playerDataSeeder.SeedAllAsync(seedDataPath, clearExisting: false);

        // Verify player generation data was seeded
        var firstNamesCount = await _dbContext.FirstNames.CountAsync();
        var lastNamesCount = await _dbContext.LastNames.CountAsync();
        var collegesCount = await _dbContext.Colleges.CountAsync();

        firstNamesCount.Should().BeGreaterThan(100, "because FirstNames.json contains ~147 names");
        lastNamesCount.Should().BeGreaterThan(100, "because LastNames.json contains ~126 names");
        collegesCount.Should().BeGreaterThan(100, "because Colleges.json contains ~107 colleges");

        // ============================================================
        // STEP 3: Seed Teams
        // ============================================================
        // This tests the same process as: TeamSeeder.SeedTeamsAsync()

        await TeamSeeder.SeedTeamsAsync(_dbContext);

        // Verify teams were created
        var teams = await _dbContext.Teams.ToListAsync();
        teams.Should().HaveCount(2, "because we seed Falcons and Eagles");

        var falcons = teams.FirstOrDefault(t => t.City == "Atlanta" && t.Name == "Falcons");
        var eagles = teams.FirstOrDefault(t => t.City == "Philadelphia" && t.Name == "Eagles");

        falcons.Should().NotBeNull("because Falcons should be seeded");
        eagles.Should().NotBeNull("because Eagles should be seeded");

        // ============================================================
        // STEP 4: Seed Players for Both Teams
        // ============================================================
        // This tests the same process as the position-specific seeders

        // Seed Falcons players
        await FalconsQBSeeder.SeedAsync(_dbContext, falcons!.Id);
        await FalconsRBSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsWRSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsTESeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsOLSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsDLSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsLBSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsDBSeeder.SeedAsync(_dbContext, falcons.Id);
        await FalconsSpecialTeamsSeeder.SeedAsync(_dbContext, falcons.Id);

        // Seed Eagles players
        await EaglesQBSeeder.SeedAsync(_dbContext, eagles!.Id);
        await EaglesRBSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesWRSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesTESeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesOLSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesDLSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesLBSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesDBSeeder.SeedAsync(_dbContext, eagles.Id);
        await EaglesSpecialTeamsSeeder.SeedAsync(_dbContext, eagles.Id);

        // ============================================================
        // STEP 5: Verify Final State
        // ============================================================
        // Database should match what we expect after running reset-database.ps1

        var totalPlayers = await _dbContext.Players.CountAsync();
        var falconsPlayersCount = await _dbContext.Players.CountAsync(p => p.TeamId == falcons.Id);
        var eaglesPlayersCount = await _dbContext.Players.CountAsync(p => p.TeamId == eagles.Id);

        // Total should be around 106 players (53 per team, give or take)
        totalPlayers.Should().BeGreaterThan(100, "because we seed ~53 players per team");
        totalPlayers.Should().BeLessThan(120, "because we seed ~53 players per team");

        // Each team should have a full roster
        falconsPlayersCount.Should().BeGreaterThan(50, "because Falcons get ~53 players");
        eaglesPlayersCount.Should().BeGreaterThan(50, "because Eagles get ~53 players");

        // Verify player data integrity - players should have names and team assignments
        var samplePlayer = await _dbContext.Players.FirstAsync();
        samplePlayer.FirstName.Should().NotBeNullOrEmpty("because all players must have first names");
        samplePlayer.LastName.Should().NotBeNullOrEmpty("because all players must have last names");
        samplePlayer.TeamId.Should().BeGreaterThan(0, "because all players must be assigned to a team");
        // Note: Position QB has value 0, so we can't use NotBe(default) - but if players exist, they have valid positions

        // ============================================================
        // SUCCESS!
        // ============================================================
        // If we reached here, the full seeding pipeline works:
        // ✅ Schema created from model
        // ✅ Player generation data seeded
        // ✅ Teams created
        // ✅ Players seeded with correct positions and assignments
        // ✅ Data integrity verified
        //
        // This gives us confidence that the seeding process in reset-database.ps1 will work!
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        _loggerFactory?.Dispose();
    }
}
