using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;
using System;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class FlowTests
    {
        private ILogger<GameFlow> CreateLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Information);
            });

            return loggerFactory.CreateLogger<GameFlow>();
        }

        [TestMethod]
        public void GameTest()
        {
            var rng = new SeedableRandom();
            var game = GameHelper.GetNewGame();
            var logger = CreateLogger();
            var gameFlow = new GameFlow(game, rng, logger);
            gameFlow.Execute();
            Assert.AreEqual(0, game.TimeRemaining);
        }

        [TestMethod]
        public void GetGraphTest()
        {
            var rng = new SeedableRandom();
            var game = GameHelper.GetNewGame();
            var logger = CreateLogger();
            var gameFlow = new GameFlow(game, rng, logger);
            var graph = gameFlow.GetGraph();
            Assert.IsNotNull(graph);
        }

        [TestMethod]
        public void GameTestWithPlayByPlayCapture()
        {
            var rng = new SeedableRandom();
            var game = GameHelper.GetNewGame();

            // Use InMemoryLogger to capture all logged messages
            var logger = new InMemoryLogger<GameFlow>();
            var gameFlow = new GameFlow(game, rng, logger);
            gameFlow.Execute();

            // Now you can read all the logged messages
            Console.WriteLine("========================================");
            Console.WriteLine($"GAME: {game.AwayTeam.City} {game.AwayTeam.Name} @ {game.HomeTeam.City} {game.HomeTeam.Name}");
            Console.WriteLine($"TOTAL PLAYS: {game.Plays.Count}");
            Console.WriteLine($"TOTAL LOG MESSAGES: {logger.LogMessages.Count}");
            Console.WriteLine("========================================");
            Console.WriteLine();

            // Print all logged messages
            foreach (var message in logger.LogMessages)
            {
                Console.WriteLine(message);
            }

            // Assertions
            Assert.AreEqual(0, game.TimeRemaining);
            Assert.IsTrue(game.Plays.Count > 0, "Game should have plays");
            Assert.IsTrue(logger.LogMessages.Count > 0, "Logger should have captured messages");
        }
    }
}
