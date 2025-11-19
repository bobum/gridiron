using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==============================================");
            Console.WriteLine("  Gridiron Database Seed Data Runner");
            Console.WriteLine("==============================================\n");
            Console.ResetColor();

            // Check if data already exists
            var existingTeams = await db.Teams.CountAsync();
            var existingPlayers = await db.Players.CountAsync();

            if (existingTeams > 0 || existingPlayers > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Database already contains {existingTeams} teams and {existingPlayers} players.");
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
                Console.WriteLine("✓ Existing data cleared.\n");
            }

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

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==============================================");
            Console.WriteLine("  Seeding Complete!");
            Console.WriteLine("==============================================");
            Console.WriteLine($"  Teams: {totalTeams}");
            Console.WriteLine($"  Total Players: {totalPlayers}");
            Console.WriteLine($"    - Falcons: {falconsCount}");
            Console.WriteLine($"    - Eagles: {eaglesCount}");
            Console.WriteLine("==============================================\n");
            Console.ResetColor();
        }
    }
}
