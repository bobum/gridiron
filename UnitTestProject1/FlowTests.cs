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
            var game = GameHelper.GetNewGame();
            var gameFlow = new GameFlow(game);
            gameFlow.Execute();
            Assert.AreEqual(0, game.TimeRemaining);
        }

        [TestMethod]
        public void GetGraphTest()
        {
            var game = GameHelper.GetNewGame();
            var gameFlow = new GameFlow(game);
            var graph = gameFlow.GetGraph();
            Assert.IsNotNull(graph);
        }
    }
}
