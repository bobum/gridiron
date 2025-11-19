using DomainObjects;
using DomainObjects.Helpers;
using DomainObjects.Time;
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
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void GameStartsInFirstQuarterTest()
        {
            var game = _testGame.GetGame();
            Assert.AreEqual(QuarterType.First, game.CurrentQuarter.QuarterType);
        }

        [TestMethod]
        public void GameStartsInFirstHalfCheckTest()
        {
            var game = _testGame.GetGame();
            Assert.AreEqual(HalfType.First, game.CurrentHalf.HalfType);
        }

        [TestMethod]
        public void AwayTeamWinsCoinTossAndDefersTest()
        {
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var rng = new TestFluentSeedableRandom()
                .NextInt(1)  // Coin toss result (1 = Away wins)
                .NextInt(1); // Defer decision (1 = defer)

            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.WonCoinToss, Possession.Away);
            Assert.IsTrue(game.DeferredPossession);
        }

        [TestMethod]
        public void AwayTeamWinsCoinTossAndReceivesTest()
        {
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var rng = new TestFluentSeedableRandom()
                .NextInt(1)  // Coin toss result (1 = Away wins)
                .NextInt(2); // Defer decision (2 = receive, any value != 1)

            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.WonCoinToss, Possession.Away);
            Assert.IsFalse(game.DeferredPossession);
        }

        [TestMethod]
        public void HomeTeamWinsCoinTossAndReceivesTest()
        {
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var rng = new TestFluentSeedableRandom()
                .NextInt(2)  // Coin toss result (2 or 0 = Home wins, any value != 1)
                .NextInt(2); // Defer decision (2 = receive, any value != 1)

            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.WonCoinToss, Possession.Home);
            Assert.IsFalse(game.DeferredPossession);
        }

        [TestMethod]
        public void HomeTeamWinsCoinTossAndDefersTest()
        {
            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            var rng = new TestFluentSeedableRandom()
                .NextInt(2)  // Coin toss result (2 or 0 = Home wins, any value != 1)
                .NextInt(1); // Defer decision (1 = defer)

            var coinToss = new CoinToss(rng);
            coinToss.Execute(game);

            Assert.AreEqual(game.WonCoinToss, Possession.Home);
            Assert.IsTrue(game.DeferredPossession);
        }

        [TestMethod]
        public void PrePlayKickoffTest()
        {
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.4); // Not used for first play (kickoff determination is based on play count)

            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            game.WonCoinToss = Possession.Away;
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession);
            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType);
        }

        [TestMethod]
        public void PrePlayKeepsPossessionTest()
        {
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.4); // Play type determination (0.4 <= 0.5 = run play)

            var game = GameHelper.GetNewGame(TestTeams.CreateTestTeams().HomeTeam, TestTeams.CreateTestTeams().VisitorTeam);
            game.WonCoinToss = Possession.Away;
            game.Plays.Add(new RunPlay() { Possession = Possession.Home });

            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession);
        }

        [TestMethod]
        public void FumbleTest()
        {
            var game = _testGame.GetGame();

            game.CurrentPlay.Possession = Possession.Home;
            var fumble = new Fumble(Possession.Away);
            fumble.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession);
            Assert.AreEqual(1, game.CurrentPlay.Fumbles.Count);
            Assert.AreEqual(true, game.CurrentPlay.PossessionChange);
        }

        [TestMethod]
        public void InterceptionTest()
        {
            var game = _testGame.GetGame();

            game.CurrentPlay.Possession = Possession.Home;
            var interception = new Interception(Possession.Away);
            interception.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession);
            Assert.AreEqual(true, game.CurrentPlay.PossessionChange);
            Assert.AreEqual(true, game.CurrentPlay.Interception);
        }

        [TestMethod]
        public void GameAdvancesQuartersAndHalvesCorrectlyTest()
        {
            var game = _testGame.GetGame();
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