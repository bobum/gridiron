using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.SeedData
{
    /// <summary>
    /// Coordinates the execution of all seed data scripts
    /// </summary>
    public static class SeedDataRunner
    {
        public static async Task RunAsync(string[] args)
        {
            // Check for force flag (non-interactive mode for CI/CD)
            bool forceMode = args.Contains("--force", StringComparer.OrdinalIgnoreCase);

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables() // Add environment variables for CI/CD
                .Build();

            // In CI/CD, prefer DefaultConnection from environment variables
            // In local dev, prefer GridironDb from appsettings/user secrets
            var defaultConnection = configuration.GetConnectionString("DefaultConnection");
            var gridironDbConnection = configuration.GetConnectionString("GridironDb");

            // Use DefaultConnection if it exists (CI/CD), otherwise fall back to GridironDb (local dev)
            // Also check if GridironDb is a placeholder value
            var connectionString = defaultConnection;
            if (string.IsNullOrEmpty(connectionString) &&
                !string.IsNullOrEmpty(gridironDbConnection) &&
                !gridironDbConnection.Contains("YOUR_SERVER"))
            {
                connectionString = gridironDbConnection;
            }

            // Debug logging for CI/CD troubleshooting
            Console.WriteLine($"Connection string source: {(defaultConnection != null ? "DefaultConnection (CI/CD)" : "GridironDb (Local)")}");
            Console.WriteLine($"Connection string value: {(connectionString != null ? $"{connectionString.Substring(0, Math.Min(50, connectionString.Length))}..." : "NULL")}");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Connection string not found.");
                Console.WriteLine("Checked: GridironDb and DefaultConnection");
                Console.WriteLine("Environment variables:");
                Console.WriteLine($"  ConnectionStrings__DefaultConnection: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")?.Substring(0, Math.Min(50, Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")?.Length ?? 0))}");
                Console.ResetColor();
                return;
            }

            // Create DbContext
            var optionsBuilder = new DbContextOptionsBuilder<GridironDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var db = new GridironDbContext(optionsBuilder.Options);

            // Create logger factory for seeding
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==============================================");
            Console.WriteLine("  Gridiron Database Seed Data Runner");
            Console.WriteLine("==============================================\n");
            Console.ResetColor();

            // Check if data already exists
            var existingTeams = await db.Teams.CountAsync();
            var existingPlayers = await db.Players.CountAsync();
            var existingFirstNames = await db.FirstNames.CountAsync();
            var existingLastNames = await db.LastNames.CountAsync();
            var existingColleges = await db.Colleges.CountAsync();

            if (existingTeams > 0 || existingPlayers > 0 || existingFirstNames > 0 || existingLastNames > 0 || existingColleges > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Database already contains:");
                Console.WriteLine($"  - {existingTeams} teams");
                Console.WriteLine($"  - {existingPlayers} players");
                Console.WriteLine($"  - {existingFirstNames} first names, {existingLastNames} last names, {existingColleges} colleges");
                Console.ResetColor();

                if (!forceMode)
                {
                    Console.Write("Do you want to clear existing data and reseed? (y/n): ");
                    var response = Console.ReadLine()?.ToLower();
                    if (response != "y")
                    {
                        Console.WriteLine("Seeding cancelled.");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Running in force mode - clearing existing data automatically...");
                }

                Console.WriteLine("Clearing existing data...");
                db.Players.RemoveRange(db.Players);
                db.Teams.RemoveRange(db.Teams);
                await db.SaveChangesAsync();
                Console.WriteLine("✓ Teams and players cleared.");

                // Clear player generation data
                await db.Database.ExecuteSqlRawAsync("DELETE FROM FirstNames");
                await db.Database.ExecuteSqlRawAsync("DELETE FROM LastNames");
                await db.Database.ExecuteSqlRawAsync("DELETE FROM Colleges");
                Console.WriteLine("✓ Player generation data cleared.\n");
            }

            // Seed God user (Global Admin) - always runs first, idempotent
            Console.WriteLine("Seeding God user...");
            await UserSeeder.SeedGodUserAsync(db);
            Console.WriteLine("✓ God user seeded.\n");

            // Seed player generation data (FirstNames, LastNames, Colleges)
            Console.WriteLine("Seeding player generation data...");
            var playerDataSeeder = new PlayerDataSeeder(db, loggerFactory.CreateLogger<PlayerDataSeeder>());

            // Get path to seed data JSON files (in same directory as executable)
            var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Gridiron.WebApi", "SeedData");
            await playerDataSeeder.SeedAllAsync(seedDataPath, clearExisting: false);
            Console.WriteLine("✓ Player generation data seeded.\n");

            // Seed teams (without players initially)
            Console.WriteLine("Creating teams...");
            await TeamSeeder.SeedTeamsAsync(db);
            Console.WriteLine("✓ Teams created.\n");

            // Get the created teams
            var falcons = await db.Teams.FirstAsync(t => t.City == "Atlanta" && t.Name == "Falcons");
            var eagles = await db.Teams.FirstAsync(t => t.City == "Philadelphia" && t.Name == "Eagles");

            // Seed Falcons players by position
            Console.WriteLine("Seeding Atlanta Falcons players...");
            await FalconsQBSeeder.SeedAsync(db, falcons.Id);
            await FalconsRBSeeder.SeedAsync(db, falcons.Id);
            await FalconsWRSeeder.SeedAsync(db, falcons.Id);
            await FalconsTESeeder.SeedAsync(db, falcons.Id);
            await FalconsOLSeeder.SeedAsync(db, falcons.Id);
            await FalconsDLSeeder.SeedAsync(db, falcons.Id);
            await FalconsLBSeeder.SeedAsync(db, falcons.Id);
            await FalconsDBSeeder.SeedAsync(db, falcons.Id);
            await FalconsSpecialTeamsSeeder.SeedAsync(db, falcons.Id);
            Console.WriteLine("✓ Falcons players seeded.\n");

            // Seed Eagles players by position
            Console.WriteLine("Seeding Philadelphia Eagles players...");
            await EaglesQBSeeder.SeedAsync(db, eagles.Id);
            await EaglesRBSeeder.SeedAsync(db, eagles.Id);
            await EaglesWRSeeder.SeedAsync(db, eagles.Id);
            await EaglesTESeeder.SeedAsync(db, eagles.Id);
            await EaglesOLSeeder.SeedAsync(db, eagles.Id);
            await EaglesDLSeeder.SeedAsync(db, eagles.Id);
            await EaglesLBSeeder.SeedAsync(db, eagles.Id);
            await EaglesDBSeeder.SeedAsync(db, eagles.Id);
            await EaglesSpecialTeamsSeeder.SeedAsync(db, eagles.Id);
            Console.WriteLine("✓ Eagles players seeded.\n");

            // Final summary
            var totalTeams = await db.Teams.CountAsync();
            var totalPlayers = await db.Players.CountAsync();
            var falconsCount = await db.Players.CountAsync(p => p.TeamId == falcons.Id);
            var eaglesCount = await db.Players.CountAsync(p => p.TeamId == eagles.Id);
            var totalFirstNames = await db.FirstNames.CountAsync();
            var totalLastNames = await db.LastNames.CountAsync();
            var totalColleges = await db.Colleges.CountAsync();
            var totalUsers = await db.Users.CountAsync();
            var godUsers = await db.Users.CountAsync(u => u.IsGlobalAdmin);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================================");
            Console.WriteLine("  Seeding Complete!");
            Console.WriteLine("==============================================");
            Console.WriteLine($"  Users: {totalUsers} ({godUsers} God)");
            Console.WriteLine($"  Teams: {totalTeams}");
            Console.WriteLine($"  Total Players: {totalPlayers}");
            Console.WriteLine($"    - Falcons: {falconsCount}");
            Console.WriteLine($"    - Eagles: {eaglesCount}");
            Console.WriteLine($"  Player Generation Data:");
            Console.WriteLine($"    - First Names: {totalFirstNames}");
            Console.WriteLine($"    - Last Names: {totalLastNames}");
            Console.WriteLine($"    - Colleges: {totalColleges}");
            Console.WriteLine("==============================================\n");
            Console.ResetColor();
        }
    }
}
