using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class GoalLineTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Own Goal Line - Safety Risk Tests

        [TestMethod]
        public void GoalLine_Own1_RunPlay_HighSkills_EscapesSafety()
        {
            // Arrange - Strong RB at own 1 yard line
            var game = CreateGameAtFieldPosition(1, Possession.Home);
            SetPlayerSkills(game, 90, 60);

            var rng = RunPlayScenarios.GoodBlocking(yards: 4); // 4 yards - escape danger

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsGreaterThan(0, runPlay.YardsGained, "Strong offense should gain positive yards");
            Assert.IsGreaterThanOrEqualTo(3, runPlay.YardsGained, $"Should escape danger zone (got {runPlay.YardsGained})");
            Assert.AreEqual(0, game.AwayScore, "No safety scored");
        }

        [TestMethod]
        public void GoalLine_Own1_RunPlay_LowSkills_SafetyRisk()
        {
            // Arrange - Weak RB at own 1 yard line - safety risk
            var game = CreateGameAtFieldPosition(1, Possession.Home);
            SetPlayerSkills(game, 30, 90); // Weak offense vs strong defense

            var rng = RunPlayScenarios.TackleForLoss(lossYards: 1); // Loss = safety

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsLessThan(0, runPlay.YardsGained, "Should lose yards");
            Assert.AreEqual(0, game.FieldPosition, "Should be tackled in end zone");
            Assert.AreEqual(2, game.AwayScore, "Safety should award 2 points to Away");
        }

        [TestMethod]
        public void GoalLine_Own1_PassPlay_Sack_ResultsInSafety()
        {
            // Arrange - QB sacked at own 1 yard line
            var game = CreateGameForPassPlay(1, Possession.Home);
            SetPassPlayerSkills(game, 40, 90);

            var rng = PassPlayScenarios.Sack(sackYards: 7); // Tries to sack for 7, clamped to 1 by field position = safety

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay!;
            Assert.AreEqual(-1, passPlay.YardsGained, "Sacked for 1 yard (clamped to field position)");
            Assert.AreEqual(0, game.FieldPosition, "Tackled in end zone");
            Assert.AreEqual(2, game.AwayScore, "Safety should award 2 points");
        }

        [TestMethod]
        public void GoalLine_Own5_RunPlay_HighSkills_GoodGain()
        {
            // Arrange - Own 5 yard line, less safety risk
            var game = CreateGameAtFieldPosition(5, Possession.Home);
            SetPlayerSkills(game, 85, 65);

            var rng = RunPlayScenarios.GoodBlocking(yards: 7); // Good gain

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsGreaterThanOrEqualTo(5, runPlay.YardsGained, $"Should gain good yards (got {runPlay.YardsGained})");
            Assert.AreEqual(0, game.AwayScore, "No safety");
        }

        [TestMethod]
        public void GoalLine_Own5_PassPlay_Complete_EscapesDanger()
        {
            // Arrange - Pass from own 5 to escape pressure
            var game = CreateGameForPassPlay(5, Possession.Home);
            SetPassPlayerSkills(game, 80, 70);

            var rng = PassPlayScenarios.CompletedPassWithYAC(airYards: 8, yacFactor: 0.5); // ~11 yards total

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay!;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete pass");
            Assert.IsGreaterThanOrEqualTo(8, passPlay.YardsGained, $"Should gain significant yards (got {passPlay.YardsGained})");
        }

        #endregion

        #region Opponent Goal Line - Touchdown Tests

        [TestMethod]
        public void GoalLine_Opponent1_RunPlay_HighSkills_Touchdown()
        {
            // Arrange - Power run from opponent 1 yard line
            var game = CreateGameAtFieldPosition(99, Possession.Home);
            SetPlayerSkills(game, 90, 70);

            var rng = RunPlayScenarios.GoodBlocking(yards: 1); // 1 yard = TD

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsGreaterThanOrEqualTo(1, runPlay.YardsGained, "Should gain at least 1 yard for TD");
            Assert.AreEqual(100, game.FieldPosition, "Should reach end zone");
            Assert.AreEqual(6, game.HomeScore, "Touchdown should award 6 points");
        }

        [TestMethod]
        public void GoalLine_Opponent1_RunPlay_LowSkills_StuffedAtGoalLine()
        {
            // Arrange - Weak offense stuffed at goal line
            var game = CreateGameAtFieldPosition(99, Possession.Home);
            SetPlayerSkills(game, 50, 90); // Weak vs strong

            // Custom skills (50 vs 90) change baseYards to 1.0 instead of 3.0, so override random value
            var rng = RunPlayScenarios.BadBlocking(yards: 0, yardsRandomOverride: 0.56); // Stuffed, 0 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.AreEqual(0, runPlay.YardsGained, "Should be stuffed");
            Assert.AreEqual(99, game.FieldPosition, "Should stay at 1 yard line");
            Assert.AreEqual(0, game.HomeScore, "No touchdown");
        }

        [TestMethod]
        public void GoalLine_Opponent1_PassPlay_Complete_Touchdown()
        {
            // Arrange - Quick slant for TD from 1 yard line
            var game = CreateGameForPassPlay(99, Possession.Home);
            SetPassPlayerSkills(game, 85, 70);

            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 1, immediateTackleYards: 0); // 1 air yard = TD

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay!;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Pass should be complete");
            Assert.IsGreaterThanOrEqualTo(1, passPlay.YardsGained, "Should gain TD");
            Assert.AreEqual(100, game.FieldPosition, "Should reach end zone");
            Assert.AreEqual(6, game.HomeScore, "Touchdown should award 6 points");
        }

        [TestMethod]
        public void GoalLine_Opponent5_RunPlay_HighSkills_Touchdown()
        {
            // Arrange - Run from 5 yard line for TD
            var game = CreateGameAtFieldPosition(95, Possession.Home);
            SetPlayerSkills(game, 90, 65);

            var rng = RunPlayScenarios.GoodBlocking(yards: 5); // 5 yards = TD

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsGreaterThanOrEqualTo(5, runPlay.YardsGained, "Should score TD");
            Assert.AreEqual(100, game.FieldPosition, "Should reach end zone");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        [TestMethod]
        public void GoalLine_Opponent5_RunPlay_LowSkills_ShortGain()
        {
            // Arrange - Weak offense from 5 yard line
            var game = CreateGameAtFieldPosition(95, Possession.Home);
            SetPlayerSkills(game, 55, 85);

            var rng = RunPlayScenarios.BadBlocking(yards: 2); // Only 2 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.IsLessThan(5, runPlay.YardsGained, "Should not reach end zone");
            Assert.IsLessThan(100, game.FieldPosition, "Should not score");
            Assert.AreEqual(0, game.HomeScore, "No touchdown");
        }

        [TestMethod]
        public void GoalLine_Opponent5_PassPlay_Complete_Touchdown()
        {
            // Arrange - Pass for TD from 5 yard line
            var game = CreateGameForPassPlay(95, Possession.Home);
            SetPassPlayerSkills(game, 88, 72);

            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 5, immediateTackleYards: 0); // 5 yards = TD

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay!;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete");
            Assert.IsGreaterThanOrEqualTo(5, passPlay.YardsGained, "Should score TD");
            Assert.AreEqual(100, game.FieldPosition, "Should reach end zone");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        #endregion

        #region Compressed Field - Goal Line Behavior Tests

        [TestMethod]
        public void GoalLine_Opponent2_PassPlay_AirYardsClampedToEndZone()
        {
            // Arrange - Deep pass from 2 yard line should be clamped
            var game = CreateGameForPassPlay(98, Possession.Home);
            SetPassPlayerSkills(game, 85, 70);

            // Try to throw 15 yards but only 2 yards to end zone
            var rng = PassPlayScenarios.DeepPass(airYards: 15, withYAC: false); // Will be clamped to 2

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay!;
            Assert.IsLessThanOrEqualTo(2,
passPlay.PassSegments[0].AirYards, $"Air yards should be clamped to end zone distance (got {passPlay.PassSegments[0].AirYards})");
        }

        [TestMethod]
        public void GoalLine_Own3_NegativeYards_SafetyPreventsOvershooting()
        {
            // Arrange - Loss at own 3 should clamp to safety, not go negative beyond 0
            var game = CreateGameAtFieldPosition(3, Possession.Home);
            SetPlayerSkills(game, 35, 90);

            var rng = RunPlayScenarios.TackleForLoss(lossYards: 5); // Would be -5 but clamped to -3

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay!;
            Assert.AreEqual(0, game.FieldPosition, "Field position should be 0 (safety)");
            Assert.IsGreaterThanOrEqualTo(-3, runPlay.YardsGained, "Loss should be clamped to field position");
            Assert.AreEqual(2, game.AwayScore, "Safety should be scored");
        }

        #endregion

        #region Matrix: Skills x Field Position x Play Type

        [TestMethod]
        public void Matrix_Own1_HighSkills_Run_EscapesSafety()
        {
            var game = CreateGameAtFieldPosition(1, Possession.Home);
            SetPlayerSkills(game, 90, 60);
            var rng = RunPlayScenarios.GoodBlocking(yards: 4);

            ExecuteRunPlayWithResult(game, rng);

            Assert.IsGreaterThan(0, game.CurrentPlay!.YardsGained, "Should escape");
            Assert.AreEqual(0, game.AwayScore, "No safety");
        }

        [TestMethod]
        public void Matrix_Own1_LowSkills_Run_Safety()
        {
            var game = CreateGameAtFieldPosition(1, Possession.Home);
            SetPlayerSkills(game, 30, 90);
            var rng = RunPlayScenarios.TackleForLoss(lossYards: 1);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(2, game.AwayScore, "Safety scored");
        }

        [TestMethod]
        public void Matrix_Own1_HighSkills_Pass_Complete()
        {
            var game = CreateGameForPassPlay(1, Possession.Home);
            SetPassPlayerSkills(game, 90, 65);
            var rng = PassPlayScenarios.CompletedPassWithYAC(airYards: 10, yacFactor: 0.7);

            ExecutePassPlayWithResult(game, rng);

            Assert.IsTrue(((PassPlay)game.CurrentPlay!).PassSegments[0].IsComplete, "Should complete");
            Assert.AreEqual(0, game.AwayScore, "No safety");
        }

        [TestMethod]
        public void Matrix_Opponent1_HighSkills_Run_Touchdown()
        {
            var game = CreateGameAtFieldPosition(99, Possession.Home);
            SetPlayerSkills(game, 90, 70);
            var rng = RunPlayScenarios.GoodBlocking(yards: 1);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(6, game.HomeScore, "Touchdown scored");
        }

        [TestMethod]
        public void Matrix_Opponent1_LowSkills_Run_Stuffed()
        {
            var game = CreateGameAtFieldPosition(99, Possession.Home);
            SetPlayerSkills(game, 50, 90);
            // Custom skills (50 vs 90) require override for precise 0 yards
            var rng = RunPlayScenarios.BadBlocking(yards: 0, yardsRandomOverride: 0.56);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(0, game.HomeScore, "No touchdown");
            Assert.AreEqual(99, game.FieldPosition, "Stuffed at goal line");
        }

        [TestMethod]
        public void Matrix_Opponent1_HighSkills_Pass_Touchdown()
        {
            var game = CreateGameForPassPlay(99, Possession.Home);
            SetPassPlayerSkills(game, 88, 70);
            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 1, immediateTackleYards: 0);

            ExecutePassPlayWithResult(game, rng);

            Assert.AreEqual(6, game.HomeScore, "Touchdown scored");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameAtFieldPosition(int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.FieldPosition = fieldPosition;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var runPlay = new RunPlay
            {
                Possession = possession,
                Down = Downs.First,
                StartFieldPosition = fieldPosition,
                ElapsedTime = 0
            };

            // Set offensive players
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

            // Set defensive players
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

        private Game CreateGameForPassPlay(int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.FieldPosition = fieldPosition;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = new PassPlay
            {
                Possession = possession,
                Down = Downs.First,
                StartFieldPosition = fieldPosition,
                ElapsedTime = 0
            };

            // Set offensive players (pass formation)
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

            // Set defensive players
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

        // Removed: Manual RNG creation methods replaced with RunPlayScenarios and PassPlayScenarios

        private void SetPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            foreach (var player in game.CurrentPlay!.OffensePlayersOnField)
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
            foreach (var player in game.CurrentPlay!.OffensePlayersOnField)
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
