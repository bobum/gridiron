using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    /// <summary>
    /// Loads teams and players from the database
    /// </summary>
    public static class TeamsLoader
    {
        /// <summary>
        /// Load teams from the database by city and name
        /// </summary>
        public static async Task<Teams> LoadFromDatabaseAsync(
            string homeCity, string homeName,
            string awayCity, string awayName,
            string? connectionString = null)
        {
            // If connection string not provided, load from configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddUserSecrets<Program>(optional: true)
                    .Build();

                connectionString = configuration.GetConnectionString("GridironDb");
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'GridironDb' not found.");
            }

            // Create DbContext
            var optionsBuilder = new DbContextOptionsBuilder<GridironDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var db = new GridironDbContext(optionsBuilder.Options);

            // Load teams with their players
            var homeTeam = await db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.City == homeCity && t.Name == homeName);

            var awayTeam = await db.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(t => t.City == awayCity && t.Name == awayName);

            if (homeTeam == null)
            {
                throw new InvalidOperationException($"Home team {homeCity} {homeName} not found in database.");
            }

            if (awayTeam == null)
            {
                throw new InvalidOperationException($"Away team {awayCity} {awayName} not found in database.");
            }

            // Return Teams object using the new database constructor
            return new Teams(homeTeam, awayTeam);
        }

        /// <summary>
        /// Load default Falcons vs Eagles matchup from database
        /// </summary>
        public static async Task<Teams> LoadDefaultMatchupAsync(string? connectionString = null)
        {
            return await LoadFromDatabaseAsync("Atlanta", "Falcons", "Philadelphia", "Eagles", connectionString);
        }
    }
}
