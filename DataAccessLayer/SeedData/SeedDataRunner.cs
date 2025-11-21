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
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("GridironDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Connection string 'GridironDb' not found.");
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
                Console.Write("Do you want to clear existing data and reseed? (y/n): ");
                Console.ResetColor();

                var response = Console.ReadLine()?.ToLower();
                if (response != "y")
                {
                    Console.WriteLine("Seeding cancelled.");
                    return;
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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================================");
            Console.WriteLine("  Seeding Complete!");
            Console.WriteLine("==============================================");
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
