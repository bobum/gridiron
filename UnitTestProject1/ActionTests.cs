using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using StateLibrary.Actions.EventChecks;
using Fumble = StateLibrary.Actions.Fumble;

namespace UnitTestProject1
{
    [TestClass]
    public class ActionTests
    {
        /// <summary>
        /// Gets a game object set to the first play (kickoff)
        /// </summary>
        /// <returns></returns>
        private Game GetGame()
        {
            var game = GameHelper.GetNewGame();
            var prePlay = new PrePlay();
            prePlay.Execute(game);
            return game;
        }

        [TestMethod]
        public void GameStartsInFirstQuarterTest()
        {
            var game = GetGame();

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);
        }

        [TestMethod]
        public void FumbleTest()
        {
            var game = GetGame();

            game.Possession = Possession.Home;
            var fumble = new Fumble(Possession.Away);
            fumble.Execute(game);
            Assert.AreEqual(Possession.Away, game.Possession);
        }

        [TestMethod]
        public void DoesQuarterExpireCheckAdvanceQuarterCorrectlyTest()
        {
            var game = GetGame();

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);
        }
    }
}