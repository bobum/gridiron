using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;

namespace UnitTestProject1
{
    [TestClass]
    public class FlowTests
    {

        [TestMethod]
        public void GameTest()
        {
            var rng = new SeedableRandom();
            var game = GameHelper.GetNewGame();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.Execute();
            Assert.AreEqual(0, game.TimeRemaining);
        }

        [TestMethod]
        public void GetGraphTest()
        {
            var rng = new SeedableRandom();
            var game = GameHelper.GetNewGame();
            var gameFlow = new GameFlow(game, rng);
            var graph = gameFlow.GetGraph();
            Assert.IsNotNull(graph);
        }
    }
}
