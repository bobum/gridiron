using DataAccessLayer;
using DomainObjects;
using static DomainObjects.StatTypes;

namespace GridironConsole;

public class DatabaseTest
{
    public static async Task TestDatabaseLoading()
    {
        Console.WriteLine("Testing database player loading...\n");

        try
        {
            // Load teams from database
            var teams = await TeamsLoader.LoadDefaultMatchupAsync();

            // Display home team
            Console.WriteLine($"Home Team: {teams.HomeTeam.City} {teams.HomeTeam.Name}");
            Console.WriteLine($"  Players: {teams.HomeTeam.Players.Count}");
            Console.WriteLine($"  Budget: ${teams.HomeTeam.Budget:N0}");
            Console.WriteLine($"  Fan Support: {teams.HomeTeam.FanSupport}");
            Console.WriteLine($"  Chemistry: {teams.HomeTeam.Chemistry}\n");

            // Display some players by position
            Console.WriteLine("Home Team Quarterbacks:");
            var qbs = teams.HomeTeam.Players.Where(p => p.Position == Positions.QB).ToList();
            foreach (var qb in qbs)
            {
                Console.WriteLine($"  #{qb.Number} {qb.FirstName} {qb.LastName} - Passing: {qb.Passing}, Speed: {qb.Speed}");
            }

            Console.WriteLine("\nHome Team Wide Receivers:");
            var wrs = teams.HomeTeam.Players.Where(p => p.Position == Positions.WR).Take(3).ToList();
            foreach (var wr in wrs)
            {
                Console.WriteLine($"  #{wr.Number} {wr.FirstName} {wr.LastName} - Catching: {wr.Catching}, Speed: {wr.Speed}");
            }

            // Display away team
            Console.WriteLine($"\nAway Team: {teams.VisitorTeam.City} {teams.VisitorTeam.Name}");
            Console.WriteLine($"  Players: {teams.VisitorTeam.Players.Count}");
            Console.WriteLine($"  Budget: ${teams.VisitorTeam.Budget:N0}");
            Console.WriteLine($"  Fan Support: {teams.VisitorTeam.FanSupport}");
            Console.WriteLine($"  Chemistry: {teams.VisitorTeam.Chemistry}\n");

            // Display some players by position
            Console.WriteLine("Away Team Quarterbacks:");
            var awayQbs = teams.VisitorTeam.Players.Where(p => p.Position == Positions.QB).ToList();
            foreach (var qb in awayQbs)
            {
                Console.WriteLine($"  #{qb.Number} {qb.FirstName} {qb.LastName} - Passing: {qb.Passing}, Speed: {qb.Speed}");
            }

            Console.WriteLine("\nAway Team Wide Receivers:");
            var awayWrs = teams.VisitorTeam.Players.Where(p => p.Position == Positions.WR).Take(3).ToList();
            foreach (var wr in awayWrs)
            {
                Console.WriteLine($"  #{wr.Number} {wr.FirstName} {wr.LastName} - Catching: {wr.Catching}, Speed: {wr.Speed}");
            }

            // Verify depth charts were built
            Console.WriteLine("\n\nDepth Charts:");
            Console.WriteLine($"Home Offense Depth Chart Positions: {teams.HomeTeam.OffenseDepthChart.Chart.Count}");
            Console.WriteLine($"Home Defense Depth Chart Positions: {teams.HomeTeam.DefenseDepthChart.Chart.Count}");
            Console.WriteLine($"Away Offense Depth Chart Positions: {teams.VisitorTeam.OffenseDepthChart.Chart.Count}");
            Console.WriteLine($"Away Defense Depth Chart Positions: {teams.VisitorTeam.DefenseDepthChart.Chart.Count}");

            Console.WriteLine("\n✓ Database loading successful!");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            Console.ResetColor();
        }
    }
}
