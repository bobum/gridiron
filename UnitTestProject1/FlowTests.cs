using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;

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
    }
}
