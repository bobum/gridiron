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
        public void DoesQuarterExpireCheckAdvanceToSecondQuarterCorrectlyTest()
        {
            var game = GetGame();

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(900, game.CurrentQuarter.TimeRemaining);
        }

        [TestMethod]
        public void DoesQuarterExpireCheckAdvanceToThirdQuarterCorrectlyTest()
        {
            var game = GetGame();
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Third, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(900, game.CurrentQuarter.TimeRemaining);
        }

        [TestMethod]
        public void DoesQuarterExpireCheckAdvanceToFourthQuarterCorrectlyTest()
        {
            var game = GetGame();
            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Third, game.CurrentQuarter.QuarterType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Fourth, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(900, game.CurrentQuarter.TimeRemaining);
        }

        [TestMethod]
        public void GameAdvancesToSecondHalfCorrectlyTest()
        {
            var game = GetGame();
            var quarterExpireCheck = new QuarterExpireCheck();
            var halftimeCheck = new HalfExpireCheck();

            Assert.AreEqual(DomainObjects.Time.QuarterType.First, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game); 
            halftimeCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Second, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game);
            halftimeCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Third, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.Second, game.CurrentHalf.HalfType);

            game.CurrentQuarter.TimeRemaining = 1;
            game.CurrentPlay.ElapsedTime = 2.0;
            quarterExpireCheck.Execute(game);
            halftimeCheck.Execute(game);

            Assert.AreEqual(DomainObjects.Time.QuarterType.Fourth, game.CurrentQuarter.QuarterType);
            Assert.AreEqual(HalfType.Second, game.CurrentHalf.HalfType);
        }
    }
}