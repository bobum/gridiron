using System.ComponentModel.DataAnnotations;
using DomainObjects;
using DomainObjects.Helpers;
using DomainObjects.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using StateLibrary.Actions.EventChecks;
using UnitTestProject1.Helpers;
using Fumble = StateLibrary.Actions.Fumble;
using Interception = StateLibrary.Actions.Interception;

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
            var rng = new CryptoRandom();
            var game = GameHelper.GetNewGame();
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);
            return game;
        }

        [TestMethod]
        public void GameStartsInFirstQuarterTest()
        {
            var game = GetGame();
            Assert.AreEqual(QuarterType.First, game.CurrentQuarter.QuarterType);
        }

        [TestMethod]
        public void GameStartsInFirstHalfCheckTest()
        {
            var game = GetGame();
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);
        }

        [TestMethod]
        public void AwayTeamWinsCoinTossTest()
        {
            var game = GetGame();
            TestCrypto rng = new TestCrypto {__NextInt = 1};
            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.Possession, Possession.Away);
        }

        [TestMethod]
        public void HomeTeamWinsCoinTossTest()
        {
            var game = GetGame();
            TestCrypto rng = new TestCrypto { __NextInt = 2 };
            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.Possession, Possession.Home);
        }

        [TestMethod]
        public void FumbleTest()
        {
            var game = GetGame();

            game.Possession = Possession.Home;
            var fumble = new Fumble(Possession.Away);
            fumble.Execute(game);

            Assert.AreEqual(Possession.Away, game.Possession);
            Assert.AreEqual(1, game.CurrentPlay.Fumbles.Count);
            Assert.AreEqual(true, game.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void InterceptionTest()
        {
            var game = GetGame();

            game.Possession = Possession.Home;
            var interception = new Interception(Possession.Away);
            interception.Execute(game);

            Assert.AreEqual(Possession.Away, game.Possession);
            Assert.AreEqual(true, game.CurrentPlay.PossessionChange);
            Assert.AreEqual(true, game.CurrentPlay.Interception);
        }

        [TestMethod]
        public void GameAdvancesQuartersAndHalvesCorrectlyTest()
        {
            var game = GetGame();
            var quarterExpireCheck = new QuarterExpireCheck();
            var halfExpireCheck = new HalfExpireCheck();

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);
            Assert.IsFalse(game.CurrentPlay.QuarterExpired);
            Assert.IsFalse(game.CurrentPlay.HalfExpired);

            //end the first quarter
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            game.CurrentPlay.QuarterExpired = false;
            game.CurrentPlay.HalfExpired = false;
            quarterExpireCheck.Execute(game); 
            halfExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);
            Assert.IsTrue(game.CurrentPlay.QuarterExpired);
            Assert.IsFalse(game.CurrentPlay.HalfExpired);

            //end the second quarter
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            game.CurrentPlay.QuarterExpired = false;
            game.CurrentPlay.HalfExpired = false;
            quarterExpireCheck.Execute(game);
            halfExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Third, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.Second, game.CurrentHalf.HalfType);
            Assert.IsTrue(game.CurrentPlay.QuarterExpired);
            Assert.IsTrue(game.CurrentPlay.HalfExpired);

            //end the third quarter
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            game.CurrentPlay.QuarterExpired = false;
            game.CurrentPlay.HalfExpired = false;
            quarterExpireCheck.Execute(game);
            halfExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Fourth, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.Second, game.CurrentHalf.HalfType);
            Assert.IsTrue(game.CurrentPlay.QuarterExpired);
            Assert.IsFalse(game.CurrentPlay.HalfExpired);

            //end the fourth quarter
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            game.CurrentPlay.QuarterExpired = false;
            game.CurrentPlay.HalfExpired = false;
            quarterExpireCheck.Execute(game);
            halfExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.GameOver, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.GameOver, game.CurrentHalf.HalfType);
            Assert.IsTrue(game.CurrentPlay.QuarterExpired);
            Assert.IsTrue(game.CurrentPlay.HalfExpired);
        }
    }
}