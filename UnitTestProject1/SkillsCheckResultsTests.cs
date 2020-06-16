using System.Linq;
using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsCheckResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class SkillsCheckResultsTests
    {
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void FumblePossessionChangeSkillsCheckResultAwayTeamTest()
        {
            var game = _testGame.GetGame();
            TestCrypto rng = new TestCrypto { __NextInt = 1 };

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Away, fumbleResult.Possession);
        }

        [TestMethod]
        public void FumblePossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            TestCrypto rng = new TestCrypto { __NextInt = 2 };

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Home, fumbleResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            game.Possession = Possession.Away;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Home, interceptionResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultAwayTeamTest()
        {
            var game = _testGame.GetGame();
            game.Possession = Possession.Home;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Away, interceptionResult.Possession);
        }

        [TestMethod]
        public void KickoffPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.PlayType = PlayType.Kickoff;
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Name);
        }

        [TestMethod]
        public void FieldGoalPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.PlayType = PlayType.FieldGoal;
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.RoughingtheKicker).Name);
        }

        [TestMethod]
        public void PuntPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.PlayType = PlayType.Punt;
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.RoughingtheKicker).Name);
        }

        [TestMethod]
        public void RunPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.PlayType = PlayType.Run;
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.OffensiveHolding).Name);
        }

        [TestMethod]
        public void PassPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.PlayType = PlayType.Pass;
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.OffensiveHolding).Name);
        }
    }
}