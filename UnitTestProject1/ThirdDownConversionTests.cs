using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class ThirdDownConversionTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Third and Long (7+ yards) Tests

        [TestMethod]
        public void ThirdAndLong_HighSkills_Pass_Converts()
        {
            // Arrange - 3rd & 10, elite QB/receivers
            var game = CreateGameAtThirdDownForPass(yardsToGo: 10, fieldPosition: 30, Possession.Home);
            SetPassPlayerSkills(game, 90, 65);

            var rng = CreateRngForCompletedPass(12, 2, protectionSucceeds: true); // 14 yards total

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete pass");
            Assert.IsTrue(passPlay.YardsGained >= 10, $"Should convert (got {passPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
        }

        [TestMethod]
        public void ThirdAndLong_LowSkills_Pass_Incomplete()
        {
            // Arrange - 3rd & 12, weak QB vs strong coverage
            var game = CreateGameAtThirdDownForPass(yardsToGo: 12, fieldPosition: 35, Possession.Home);
            SetPassPlayerSkills(game, 50, 85);

            var rng = CreateRngForIncompletePass(protectionSucceeds: false);

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Should be incomplete");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
        }

        [TestMethod]
        public void ThirdAndLong_HighSkills_Run_RiskyButPossible()
        {
            // Arrange - 3rd & 8, elite RB tries to run (risky)
            var game = CreateGameAtThirdDown(yardsToGo: 8, fieldPosition: 40, Possession.Home);
            SetPlayerSkills(game, 92, 70);

            var rng = CreateRngForRunPlay(9, blockingSucceeds: true); // Barely converts

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 8, $"Elite RB can convert 3rd & long (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should convert");
        }

        [TestMethod]
        public void ThirdAndLong_LowSkills_Run_Fails()
        {
            // Arrange - 3rd & 10, weak RB vs strong defense
            var game = CreateGameAtThirdDown(yardsToGo: 10, fieldPosition: 25, Possession.Home);
            SetPlayerSkills(game, 50, 85);

            var rng = CreateRngForRunPlay(4, blockingSucceeds: false); // Only 4 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained < 10, $"Should not convert (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
        }

        #endregion

        #region Third and Short (1-3 yards) Tests

        [TestMethod]
        public void ThirdAndShort_HighSkills_PowerRun_Converts()
        {
            // Arrange - 3rd & 1, power run formation
            var game = CreateGameAtThirdDown(yardsToGo: 1, fieldPosition: 45, Possession.Home);
            SetPlayerSkills(game, 88, 70);

            var rng = CreateRngForRunPlay(2, blockingSucceeds: true); // Power through

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 1, $"Should convert QB sneak/power run (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
        }

        [TestMethod]
        public void ThirdAndShort_LowSkills_Run_Stuffed()
        {
            // Arrange - 3rd & 2, weak O-line vs strong D-line
            var game = CreateGameAtThirdDown(yardsToGo: 2, fieldPosition: 50, Possession.Home);
            SetPlayerSkills(game, 45, 90);

            var rng = CreateRngForRunPlay(1, blockingSucceeds: false); // Only 1 yard

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained < 2, $"Should be stuffed (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
        }

        [TestMethod]
        public void ThirdAndShort_Pass_QuickSlant_Converts()
        {
            // Arrange - 3rd & 2, quick pass instead of run
            var game = CreateGameAtThirdDownForPass(yardsToGo: 2, fieldPosition: 42, Possession.Home);
            SetPassPlayerSkills(game, 80, 75);

            var rng = CreateRngForCompletedPass(5, 1, protectionSucceeds: true); // 6 yards

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Quick pass should complete");
            Assert.IsTrue(passPlay.YardsGained >= 2, $"Should convert (got {passPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
        }

        #endregion

        #region Third and Medium (4-6 yards) Tests

        [TestMethod]
        public void ThirdAndMedium_HighSkills_Run_Converts()
        {
            // Arrange - 3rd & 5, skilled RB
            var game = CreateGameAtThirdDown(yardsToGo: 5, fieldPosition: 38, Possession.Home);
            SetPlayerSkills(game, 85, 68);

            var rng = CreateRngForRunPlay(6, blockingSucceeds: true); // 6 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 5, $"Should convert (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
        }

        [TestMethod]
        public void ThirdAndMedium_HighSkills_Pass_Converts()
        {
            // Arrange - 3rd & 6, balanced offense
            var game = CreateGameAtThirdDownForPass(yardsToGo: 6, fieldPosition: 33, Possession.Home);
            SetPassPlayerSkills(game, 82, 70);

            var rng = CreateRngForCompletedPass(8, 2, protectionSucceeds: true); // 10 yards

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete");
            Assert.IsTrue(passPlay.YardsGained >= 6, $"Should convert (got {passPlay.YardsGained})");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
        }

        [TestMethod]
        public void ThirdAndMedium_LowSkills_Pass_Incomplete()
        {
            // Arrange - 3rd & 4, weak QB/receivers
            var game = CreateGameAtThirdDownForPass(yardsToGo: 4, fieldPosition: 28, Possession.Home);
            SetPassPlayerSkills(game, 55, 80);

            var rng = CreateRngForIncompletePass(protectionSucceeds: false);

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Should be incomplete");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
        }

        [TestMethod]
        public void ThirdAndMedium_LowSkills_Run_ShortGain()
        {
            // Arrange - 3rd & 5, weak offense
            var game = CreateGameAtThirdDown(yardsToGo: 5, fieldPosition: 35, Possession.Home);
            SetPlayerSkills(game, 52, 82);

            var rng = CreateRngForRunPlay(3, blockingSucceeds: false); // Only 3 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained < 5, $"Should not convert (got {runPlay.YardsGained})");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
        }

        #endregion

        #region Matrix: Distance x Skills x Play Type

        [TestMethod]
        public void Matrix_ThirdAndLong_HighSkills_Pass_Success()
        {
            var game = CreateGameAtThirdDownForPass(10, 30, Possession.Home);
            SetPassPlayerSkills(game, 90, 65);
            var rng = CreateRngForCompletedPass(12, 3, protectionSucceeds: true);

            ExecutePassPlayWithResult(game, rng);

            Assert.AreEqual(Downs.First, game.CurrentDown, "Should convert");
        }

        [TestMethod]
        public void Matrix_ThirdAndLong_LowSkills_Pass_Fail()
        {
            var game = CreateGameAtThirdDownForPass(10, 30, Possession.Home);
            SetPassPlayerSkills(game, 50, 85);
            var rng = CreateRngForIncompletePass(false);

            ExecutePassPlayWithResult(game, rng);

            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should fail");
        }

        [TestMethod]
        public void Matrix_ThirdAndShort_HighSkills_Run_Success()
        {
            var game = CreateGameAtThirdDown(1, 45, Possession.Home);
            SetPlayerSkills(game, 88, 70);
            var rng = CreateRngForRunPlay(2, blockingSucceeds: true);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(Downs.First, game.CurrentDown, "Should convert");
        }

        [TestMethod]
        public void Matrix_ThirdAndShort_LowSkills_Run_Fail()
        {
            var game = CreateGameAtThirdDown(2, 45, Possession.Home);
            SetPlayerSkills(game, 45, 90);
            var rng = CreateRngForRunPlay(1, blockingSucceeds: false);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should fail");
        }

        [TestMethod]
        public void Matrix_ThirdAndMedium_HighSkills_Run_Success()
        {
            var game = CreateGameAtThirdDown(5, 38, Possession.Home);
            SetPlayerSkills(game, 85, 68);
            var rng = CreateRngForRunPlay(6, blockingSucceeds: true);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(Downs.First, game.CurrentDown, "Should convert");
        }

        [TestMethod]
        public void Matrix_ThirdAndMedium_LowSkills_Pass_Fail()
        {
            var game = CreateGameAtThirdDownForPass(5, 35, Possession.Home);
            SetPassPlayerSkills(game, 55, 80);
            var rng = CreateRngForIncompletePass(false);

            ExecutePassPlayWithResult(game, rng);

            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should fail");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameAtThirdDown(int yardsToGo, int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.FieldPosition = fieldPosition;
            game.YardsToGo = yardsToGo;
            game.CurrentDown = Downs.Third;

            var runPlay = new RunPlay
            {
                Possession = possession,
                Down = Downs.Third,
                StartFieldPosition = fieldPosition,
                ElapsedTime = 0
            };

            runPlay.OffensePlayersOnField = new List<Player>
            {
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.FB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.C][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            runPlay.DefensePlayersOnField = new List<Player>
            {
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][2],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = runPlay;
            return game;
        }

        private Game CreateGameAtThirdDownForPass(int yardsToGo, int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.FieldPosition = fieldPosition;
            game.YardsToGo = yardsToGo;
            game.CurrentDown = Downs.Third;

            var passPlay = new PassPlay
            {
                Possession = possession,
                Down = Downs.Third,
                StartFieldPosition = fieldPosition,
                ElapsedTime = 0
            };

            passPlay.OffensePlayersOnField = new List<Player>
            {
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.C][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][2],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            passPlay.DefensePlayersOnField = new List<Player>
            {
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = passPlay;
            return game;
        }

        private TestFluentSeedableRandom CreateRngForRunPlay(int desiredYards, bool blockingSucceeds)
        {
            double targetBase = blockingSucceeds
                ? Math.Ceiling(desiredYards / 1.2)
                : Math.Ceiling(desiredYards / 0.8);

            double randomFactor = targetBase - 3.0;
            double nextDouble = (randomFactor + 15.0) / 25.0;
            nextDouble = Math.Max(0.0, Math.Min(1.0, nextDouble));

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)
                .NextInt(2)
                .RunBlockingCheck(blockingSucceeds ? 0.3 : 0.7)
                .NextDouble(nextDouble)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.9)
                .NextDouble(0.99)                     // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForCompletedPass(int airYards, int yacYards, bool protectionSucceeds)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(protectionSucceeds ? 0.3 : 0.7)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(airYards)
                .PassCompletionCheck(0.4)
                .ImmediateTackleYards(2)
                .YACOpportunityCheck(0.3)
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)
                .NextDouble(0.99)                     // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForIncompletePass(bool protectionSucceeds)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(protectionSucceeds ? 0.3 : 0.7)
                .QBPressureCheck(0.8)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(10)
                .PassCompletionCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
        }

        private void SetPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = offenseSkill;
                player.Rushing = offenseSkill;
                player.Speed = offenseSkill;
                player.Agility = offenseSkill;
                player.Strength = offenseSkill;
            }

            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = defenseSkill;
                player.Strength = defenseSkill;
                player.Speed = defenseSkill;
            }
        }

        private void SetPassPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = offenseSkill;
                player.Passing = offenseSkill;
                player.Catching = offenseSkill;
                player.Speed = offenseSkill;
                player.Agility = offenseSkill;
                player.Awareness = offenseSkill;
                player.Rushing = offenseSkill;
            }

            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = defenseSkill;
                player.Coverage = defenseSkill;
                player.Strength = defenseSkill;
                player.Speed = defenseSkill;
                player.Awareness = defenseSkill;
            }
        }

        private void ExecuteRunPlayWithResult(Game game, ISeedableRandom rng)
        {
            var run = new Run(rng);
            run.Execute(game);

            var runResult = new RunResult();
            runResult.Execute(game);
        }

        private void ExecutePassPlayWithResult(Game game, ISeedableRandom rng)
        {
            var pass = new Pass(rng);
            pass.Execute(game);

            var passResult = new PassResult();
            passResult.Execute(game);
        }

        #endregion
    }
}
