using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary;

namespace GridironConsole;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("================================================================================");
        Console.WriteLine("                    GRIDIRON FOOTBALL SIMULATION");
        Console.WriteLine("================================================================================");
        Console.WriteLine();

        // Create a new randomized game
        var rng = new SeedableRandom();
        var game = GameHelper.GetNewGame();

        // Display matchup
        Console.WriteLine($"MATCHUP: {game.AwayTeam.City} {game.AwayTeam.Name} @ {game.HomeTeam.City} {game.HomeTeam.Name}");
        Console.WriteLine($"Location: {game.HomeTeam.Stadium}");
        Console.WriteLine();
        Console.WriteLine("Press ENTER to start the game...");
        Console.ReadLine();
        Console.WriteLine();

        // Set up logger to display play-by-play
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
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

        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine("                         GAME STATISTICS");
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        Console.WriteLine($"Total Plays: {game.Plays.Count}");
        Console.WriteLine($"Game Duration: {FormatTime(3600 - game.TimeRemaining)}");

        // Count play types
        int passPlays = 0, runPlays = 0, kickoffs = 0, punts = 0, fieldGoals = 0;
        int completions = 0, interceptions = 0, fumbles = 0;

        foreach (var play in game.Plays)
        {
            if (play is PassPlay passPlay)
            {
                passPlays++;
                if (passPlay.PassCompleted) completions++;
                if (passPlay.Interception) interceptions++;
            }
            else if (play is RunPlay) runPlays++;
            else if (play is KickoffPlay) kickoffs++;
            else if (play is PuntPlay) punts++;
            else if (play is FieldGoalPlay) fieldGoals++;

            if (play.Fumbles.Count > 0) fumbles++;
        }

        Console.WriteLine();
        Console.WriteLine("Play Breakdown:");
        Console.WriteLine($"  Pass Plays: {passPlays} ({completions} completions, {(passPlays > 0 ? (completions * 100.0 / passPlays).ToString("F1") : "0")}% completion rate)");
        Console.WriteLine($"  Run Plays: {runPlays}");
        Console.WriteLine($"  Kickoffs: {kickoffs}");
        Console.WriteLine($"  Punts: {punts}");
        Console.WriteLine($"  Field Goals: {fieldGoals}");
        Console.WriteLine();
        Console.WriteLine($"Turnovers:");
        Console.WriteLine($"  Interceptions: {interceptions}");
        Console.WriteLine($"  Fumbles: {fumbles}");
        Console.WriteLine();
        Console.WriteLine("================================================================================");
        Console.WriteLine();
        Console.WriteLine("Press ENTER to exit...");
        Console.ReadLine();
    }

    static string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }
}
