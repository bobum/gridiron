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
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 1}};

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Away, fumbleResult.Possession);
        }

        [TestMethod]
        public void FumblePossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom {__NextInt = {[0] = 2}};

            var fumbleResult = new FumblePossessionChangeSkillsCheckResult(rng);
            fumbleResult.Execute(game);

            Assert.AreEqual(Possession.Home, fumbleResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultHomeTeamTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.Possession = Possession.Away;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Home, interceptionResult.Possession);
        }

        [TestMethod]
        public void InterceptionPossessionChangeSkillsCheckResultAwayTeamTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay.Possession = Possession.Home;

            var interceptionResult = new InterceptionPossessionChangeSkillsCheckResult();
            interceptionResult.Execute(game);

            Assert.AreEqual(Possession.Away, interceptionResult.Possession);
        }

        [TestMethod]
        public void KickoffPenaltySkillsCheckResultTest()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new KickoffPlay();
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
            game.CurrentPlay = new FieldGoalPlay();
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
            game.CurrentPlay = new PuntPlay();
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
            game.CurrentPlay = new RunPlay();
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
            game.CurrentPlay = new PassPlay();
            var penalty = new Penalty();
            var penaltySkillsCheckResult = new PenaltySkillsCheckResult(penalty);
            penaltySkillsCheckResult.Execute(game);

            Assert.AreEqual(game.CurrentPlay.Penalties.First().Name,
                Penalties.List.Single(p => p.Name == PenaltyNames.OffensiveHolding).Name);
        }

        #region AirYardsSkillsCheckResult Tests

        [TestMethod]
        public void AirYardsSkillsCheckResult_ScreenPass_ReturnsNegativeToShortYards()
        {
            // Arrange
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 1 } }; // Will return 1 from Next(-3, 3)

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Screen, 25);
            airYardsResult.Execute(game);

            // Assert - Screen passes range from -3 to 2 yards
            Assert.IsTrue(airYardsResult.Result >= -3);
            Assert.IsTrue(airYardsResult.Result < 3);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ShortPass_Returns3To11Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 5 } }; // Will return 5 from Next(3, 12)

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Short, 25);
            airYardsResult.Execute(game);

            // Assert - Short passes range from 3 to 11 yards
            Assert.IsTrue(airYardsResult.Result >= 3);
            Assert.IsTrue(airYardsResult.Result < 12);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ForwardPass_Returns8To19Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 12 } }; // Will return 12 from Next(8, 20)

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Forward, 25);
            airYardsResult.Execute(game);

            // Assert - Forward passes range from 8 to 19 yards
            Assert.IsTrue(airYardsResult.Result >= 8);
            Assert.IsTrue(airYardsResult.Result < 20);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_DeepPass_Returns18To44Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 30 } }; // Will return 30 from Next(18, 45)

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Deep, 25);
            airYardsResult.Execute(game);

            // Assert - Deep passes range from 18 to 44 yards
            Assert.IsTrue(airYardsResult.Result >= 18);
            Assert.IsTrue(airYardsResult.Result < 45);
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_DeepPassNearGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At the 10 yard line, only 90 yards to goal
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 50 } }; // Would normally return 50, but clamped

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Deep, 90);
            airYardsResult.Execute(game);

            // Assert - Should be clamped to yards to goal (10 yards)
            // Deep pass at 90 yard line: Next(18, Max(19, Min(45, 10))) = Next(18, 19)
            Assert.IsTrue(airYardsResult.Result >= 18);
            Assert.IsTrue(airYardsResult.Result < 19); // Effectively just 18
        }

        [TestMethod]
        public void AirYardsSkillsCheckResult_ShortPassNearGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At the 95 yard line, only 5 yards to goal
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 4 } };

            // Act
            var airYardsResult = new AirYardsSkillsCheckResult(rng, PassType.Short, 95);
            airYardsResult.Execute(game);

            // Assert - Short pass at 95 yard line: Next(3, Max(4, Min(12, 5))) = Next(3, 5)
            Assert.IsTrue(airYardsResult.Result >= 3);
            Assert.IsTrue(airYardsResult.Result < 5);
        }

        #endregion

        #region SackYardsSkillsCheckResult Tests

        [TestMethod]
        public void SackYardsSkillsCheckResult_MidfieldSack_ReturnsNegative2To10Yards()
        {
            // Arrange
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 5 } }; // Will return -5 yards

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 50); // At midfield
            sackResult.Execute(game);

            // Assert - Should return negative yards (loss)
            Assert.IsTrue(sackResult.Result < 0);
            Assert.IsTrue(sackResult.Result >= -10);
            Assert.IsTrue(sackResult.Result <= -2);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_NearOwnGoalLine_ClampedToFieldPosition()
        {
            // Arrange - At own 5 yard line
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 10 } }; // Would lose 10 yards

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 5);
            sackResult.Execute(game);

            // Assert - Can't lose more than 5 yards (would be safety)
            Assert.AreEqual(-5, sackResult.Result);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_AtOwnGoalLine_ReturnsZero()
        {
            // Arrange - At own goal line (safety situation)
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 7 } };

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 0);
            sackResult.Execute(game);

            // Assert - Already at goal line, can't lose yards
            Assert.AreEqual(0, sackResult.Result);
        }

        [TestMethod]
        public void SackYardsSkillsCheckResult_DeepInOwnTerritory_AllowsFullLoss()
        {
            // Arrange - Deep in own territory at 25 yard line
            var game = _testGame.GetGame();
            TestSeedableRandom rng = new TestSeedableRandom { __NextInt = { [0] = 8 } }; // 8 yard loss

            // Act
            var sackResult = new SackYardsSkillsCheckResult(rng, 25);
            sackResult.Execute(game);

            // Assert - Full loss allowed (not near goal line)
            Assert.AreEqual(-8, sackResult.Result);
        }

        #endregion
    }
}