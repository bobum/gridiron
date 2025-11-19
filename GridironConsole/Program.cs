using DomainObjects;
using DomainObjects.Helpers;
using DataAccessLayer;
using Microsoft.Extensions.Logging;
using StateLibrary;

namespace GridironConsole;

class Program
{
    static async Task Main(string[] args)
    {
        // Option to test database loading first
        if (args.Length > 0 && args[0] == "--test-db")
        {
            await DatabaseTest.TestDatabaseLoading();
            return;
        }

        Console.WriteLine("================================================================================");
        Console.WriteLine("                    GRIDIRON FOOTBALL SIMULATION");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        // Prompt user for database vs JSON loading
        Console.WriteLine("Load teams from:");
        Console.WriteLine("  1. Database (default)");
        Console.WriteLine("  2. JSON (legacy)");
        Console.Write("\nEnter choice (1 or 2): ");
        var choice = Console.ReadLine();

        Game game;
        if (choice == "2")
        {
            Console.WriteLine("Loading teams from JSON...");
            game = GameHelper.GetNewGame();
        }
        else
        {
            Console.WriteLine("Loading teams from database...");
            var teams = await TeamsLoader.LoadDefaultMatchupAsync();
            game = new Game
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };
        }

        // Create a new randomized game
        var rng = new SeedableRandom();

        // Display matchup
        Console.WriteLine($"\nMATCHUP: {game.AwayTeam.City} {game.AwayTeam.Name} @ {game.HomeTeam.City} {game.HomeTeam.Name}");
        Console.WriteLine($"Home Players: {game.HomeTeam.Players.Count}, Away Players: {game.AwayTeam.Players.Count}");
        Console.WriteLine();
        Console.WriteLine("Press ENTER to start the game...");
        Console.ReadLine();
        Console.WriteLine();

        // Set up clean logger that only outputs messages (no log level, category, etc.)
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddCleanConsole(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<GameFlow>();

        // Create and execute game
        var gameFlow = new GameFlow(game, rng, logger);

        Console.WriteLine("Starting game simulation...");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        gameFlow.Execute();

        // Display final results
        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine("                         FINAL SCORE");
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        Console.WriteLine($"{game.AwayTeam.City} {game.AwayTeam.Name}: {game.AwayScore}");
        Console.WriteLine($"{game.HomeTeam.City} {game.HomeTeam.Name}: {game.HomeScore}");
        Console.WriteLine();

        var winner = game.HomeScore > game.AwayScore ? $"{game.HomeTeam.City} {game.HomeTeam.Name}" :
                     game.AwayScore > game.HomeScore ? $"{game.AwayTeam.City} {game.AwayTeam.Name}" : "TIE";

        if (winner != "TIE")
        {
            Console.WriteLine($"WINNER: {winner}");
        }
        else
        {
            Console.WriteLine("RESULT: TIE GAME");
        }

        DisplayGameStats(game);

        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();
    }

    static void DisplayGameStats(Game game)
    {
        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine("                         GAME STATISTICS");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        var allPlayers = game.HomeTeam.Players.Concat(game.AwayTeam.Players).ToList();

        // Passing Leaders
        Console.WriteLine("PASSING LEADERS");
        Console.WriteLine("--------------------------------------------------------------------------------");
        var passers = allPlayers.Where(p => p.Stats.ContainsKey(StatTypes.PlayerStatType.PassingYards) && p.Stats[StatTypes.PlayerStatType.PassingYards] > 0)
            .OrderByDescending(p => p.Stats[StatTypes.PlayerStatType.PassingYards])
            .Take(5);
        
        foreach (var p in passers)
        {
            var yards = p.Stats.ContainsKey(StatTypes.PlayerStatType.PassingYards) ? p.Stats[StatTypes.PlayerStatType.PassingYards] : 0;
            var tds = p.Stats.ContainsKey(StatTypes.PlayerStatType.PassingTouchdowns) ? p.Stats[StatTypes.PlayerStatType.PassingTouchdowns] : 0;
            var ints = p.Stats.ContainsKey(StatTypes.PlayerStatType.InterceptionsThrown) ? p.Stats[StatTypes.PlayerStatType.InterceptionsThrown] : 0;
            var att = p.Stats.ContainsKey(StatTypes.PlayerStatType.PassingAttempts) ? p.Stats[StatTypes.PlayerStatType.PassingAttempts] : 0;
            var comp = p.Stats.ContainsKey(StatTypes.PlayerStatType.PassingCompletions) ? p.Stats[StatTypes.PlayerStatType.PassingCompletions] : 0;
            var team = game.HomeTeam.Players.Contains(p) ? game.HomeTeam.Name : game.AwayTeam.Name;
            
            Console.WriteLine($"  [{team}] {p.Position} #{p.Number} {p.LastName}: {comp}/{att}, {yards} yds, {tds} TD, {ints} INT");
        }
        Console.WriteLine();

        // Rushing Leaders
        Console.WriteLine("RUSHING LEADERS");
        Console.WriteLine("--------------------------------------------------------------------------------");
        var rushers = allPlayers.Where(p => p.Stats.ContainsKey(StatTypes.PlayerStatType.RushingYards) && p.Stats[StatTypes.PlayerStatType.RushingYards] != 0)
            .OrderByDescending(p => p.Stats[StatTypes.PlayerStatType.RushingYards])
            .Take(5);

        foreach (var p in rushers)
        {
            var yards = p.Stats.ContainsKey(StatTypes.PlayerStatType.RushingYards) ? p.Stats[StatTypes.PlayerStatType.RushingYards] : 0;
            var tds = p.Stats.ContainsKey(StatTypes.PlayerStatType.RushingTouchdowns) ? p.Stats[StatTypes.PlayerStatType.RushingTouchdowns] : 0;
            var att = p.Stats.ContainsKey(StatTypes.PlayerStatType.RushingAttempts) ? p.Stats[StatTypes.PlayerStatType.RushingAttempts] : 0;
            var team = game.HomeTeam.Players.Contains(p) ? game.HomeTeam.Name : game.AwayTeam.Name;

            Console.WriteLine($"  [{team}] {p.Position} #{p.Number} {p.LastName}: {att} att, {yards} yds, {tds} TD");
        }
        Console.WriteLine();

        // Receiving Leaders
        Console.WriteLine("RECEIVING LEADERS");
        Console.WriteLine("--------------------------------------------------------------------------------");
        var receivers = allPlayers.Where(p => p.Stats.ContainsKey(StatTypes.PlayerStatType.ReceivingYards) && p.Stats[StatTypes.PlayerStatType.ReceivingYards] > 0)
            .OrderByDescending(p => p.Stats[StatTypes.PlayerStatType.ReceivingYards])
            .Take(5);

        foreach (var p in receivers)
        {
            var yards = p.Stats.ContainsKey(StatTypes.PlayerStatType.ReceivingYards) ? p.Stats[StatTypes.PlayerStatType.ReceivingYards] : 0;
            var tds = p.Stats.ContainsKey(StatTypes.PlayerStatType.ReceivingTouchdowns) ? p.Stats[StatTypes.PlayerStatType.ReceivingTouchdowns] : 0;
            var rec = p.Stats.ContainsKey(StatTypes.PlayerStatType.Receptions) ? p.Stats[StatTypes.PlayerStatType.Receptions] : 0;
            var team = game.HomeTeam.Players.Contains(p) ? game.HomeTeam.Name : game.AwayTeam.Name;

            Console.WriteLine($"  [{team}] {p.Position} #{p.Number} {p.LastName}: {rec} rec, {yards} yds, {tds} TD");
        }
        Console.WriteLine();

        // Defensive Leaders
        Console.WriteLine("DEFENSIVE LEADERS");
        Console.WriteLine("--------------------------------------------------------------------------------");
        var defenders = allPlayers.Where(p => 
            (p.Stats.ContainsKey(StatTypes.PlayerStatType.Tackles) && p.Stats[StatTypes.PlayerStatType.Tackles] > 0) ||
            (p.Stats.ContainsKey(StatTypes.PlayerStatType.Sacks) && p.Stats[StatTypes.PlayerStatType.Sacks] > 0) ||
            (p.Stats.ContainsKey(StatTypes.PlayerStatType.InterceptionsCaught) && p.Stats[StatTypes.PlayerStatType.InterceptionsCaught] > 0))
            .OrderByDescending(p => (p.Stats.ContainsKey(StatTypes.PlayerStatType.Tackles) ? p.Stats[StatTypes.PlayerStatType.Tackles] : 0) + 
                                    (p.Stats.ContainsKey(StatTypes.PlayerStatType.Sacks) ? p.Stats[StatTypes.PlayerStatType.Sacks] * 2 : 0))
            .Take(5);

        foreach (var p in defenders)
        {
            var tackles = p.Stats.ContainsKey(StatTypes.PlayerStatType.Tackles) ? p.Stats[StatTypes.PlayerStatType.Tackles] : 0;
            var sacks = p.Stats.ContainsKey(StatTypes.PlayerStatType.Sacks) ? p.Stats[StatTypes.PlayerStatType.Sacks] : 0;
            var ints = p.Stats.ContainsKey(StatTypes.PlayerStatType.InterceptionsCaught) ? p.Stats[StatTypes.PlayerStatType.InterceptionsCaught] : 0;
            var team = game.HomeTeam.Players.Contains(p) ? game.HomeTeam.Name : game.AwayTeam.Name;

            Console.WriteLine($"  [{team}] {p.Position} #{p.Number} {p.LastName}: {tackles} tackles, {sacks} sacks, {ints} INT");
        }
        Console.WriteLine();
    }

    static string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }
}
