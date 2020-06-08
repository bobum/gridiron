using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;
using StateLibrary.Actions;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GameTest()
        {
            var game = GameHelper.GetNewGame();
            var gameFlow = new GameFlow(game);
            Assert.AreEqual(0, game.TimeRemaining);
        }
    }
}
