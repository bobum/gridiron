using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;
using StateLibrary.Actions;

namespace UnitTestProject1
{
    [TestClass]
    public class ActionTests
    {
        [TestMethod]
        public void FumbleTest()
        {
            var game = GameHelper.GetNewGame();
            var prePlay = new PrePlay();
            prePlay.Execute(game);
            game.Possession = Possession.Home;
            var fumble = new Fumble(Possession.Away);
            fumble.Execute(game);
            Assert.AreEqual(Possession.Away, game.Possession);
        }
    }
}