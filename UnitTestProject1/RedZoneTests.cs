using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class RedZoneTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Red Zone - 20 Yard Line Tests

        [TestMethod]
        public void RedZone_At20_HighSkills_Run_GoodGain()
        {
            // Arrange - At the 20, run game is effective
            var game = CreateGameInRedZone(80, Possession.Home);
            SetPlayerSkills(game, 85, 68);

            var rng = RunPlayScenarios.GoodBlocking(yards: 8);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 6, $"Run should be effective in red zone (got {runPlay.YardsGained})");
        }

        [TestMethod]
        public void RedZone_At20_HighSkills_Pass_FullField()
        {
            // Arrange - At 20, can still throw deep
            var game = CreateGameInRedZoneForPass(80, Possession.Home);
            SetPassPlayerSkills(game, 88, 70);

            var rng = PassPlayScenarios.CompletedPassWithYAC(airYards: 15, yacFactor: 0.5); // ~18 yards total

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete");
            Assert.IsTrue(passPlay.YardsGained >= 15, $"Can still throw deep from 20 (got {passPlay.YardsGained})");
        }

        #endregion

        #region Red Zone - 15 Yard Line Tests

        [TestMethod]
        public void RedZone_At15_HighSkills_Run_PowerGame()
        {
            // Arrange - At 15, run game becomes more prominent
            var game = CreateGameInRedZone(85, Possession.Home);
            SetPlayerSkills(game, 88, 70);

            var rng = RunPlayScenarios.GoodBlocking(yards: 7);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 6, $"Power run should work (got {runPlay.YardsGained})");
        }

        [TestMethod]
        public void RedZone_At15_Pass_LimitedAirYards()
        {
            // Arrange - At 15, pass routes compressed
            var game = CreateGameInRedZoneForPass(85, Possession.Home);
            SetPassPlayerSkills(game, 82, 72);

            var rng = PassPlayScenarios.CompletedPassWithYAC(airYards: 10, yacFactor: 0.5);

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].AirYards <= 15,
                $"Air yards limited by field (got {passPlay.PassSegments[0].AirYards})");
        }

        #endregion

        #region Red Zone - 10 Yard Line Tests

        [TestMethod]
        public void RedZone_At10_HighSkills_Run_Touchdown()
        {
            // Arrange - At 10, good chance for TD on run
            var game = CreateGameInRedZone(90, Possession.Home);
            SetPlayerSkills(game, 90, 68);

            var rng = RunPlayScenarios.GoodBlocking(yards: 10); // TD!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 10, "Should score TD");
            Assert.AreEqual(100, game.FieldPosition, "Should reach end zone");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        [TestMethod]
        public void RedZone_At10_LowSkills_Run_ShortGain()
        {
            // Arrange - At 10, weak offense struggles
            var game = CreateGameInRedZone(90, Possession.Home);
            SetPlayerSkills(game, 55, 85);

            var rng = RunPlayScenarios.BadBlocking(yards: 3);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained < 10, $"Should not score (got {runPlay.YardsGained})");
            Assert.AreEqual(0, game.HomeScore, "No touchdown");
        }

        [TestMethod]
        public void RedZone_At10_Pass_TouchdownPass()
        {
            // Arrange - At 10, fade route for TD
            var game = CreateGameInRedZoneForPass(90, Possession.Home);
            SetPassPlayerSkills(game, 88, 70);

            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 10, immediateTackleYards: 0); // TD pass

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "TD pass should complete");
            Assert.IsTrue(passPlay.YardsGained >= 10, "Should score");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        #endregion

        #region Red Zone - 5 Yard Line Tests

        [TestMethod]
        public void RedZone_At5_HighSkills_Run_HighTDProbability()
        {
            // Arrange - At 5, elite RB should score
            var game = CreateGameInRedZone(95, Possession.Home);
            SetPlayerSkills(game, 92, 70);

            var rng = RunPlayScenarios.GoodBlocking(yards: 5);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 5, "Elite RB should score from 5");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        [TestMethod]
        public void RedZone_At5_LowSkills_Run_Stopped()
        {
            // Arrange - At 5, weak offense stopped
            var game = CreateGameInRedZone(95, Possession.Home);
            SetPlayerSkills(game, 50, 88);

            var rng = RunPlayScenarios.BadBlocking(yards: 2);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained < 5, $"Should be stopped (got {runPlay.YardsGained})");
            Assert.AreEqual(0, game.HomeScore, "No touchdown");
        }

        [TestMethod]
        public void RedZone_At5_Pass_QuickSlantTouchdown()
        {
            // Arrange - Quick slant from 5
            var game = CreateGameInRedZoneForPass(95, Possession.Home);
            SetPassPlayerSkills(game, 85, 72);

            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 5, immediateTackleYards: 0);

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Quick slant completes");
            Assert.AreEqual(6, game.HomeScore, "Touchdown!");
        }

        #endregion

        #region Matrix: Field Position x Skills x Play Type

        [TestMethod]
        public void Matrix_RedZone20_HighSkills_Run_Success()
        {
            var game = CreateGameInRedZone(80, Possession.Home);
            SetPlayerSkills(game, 85, 68);
            var rng = RunPlayScenarios.GoodBlocking(yards: 8);

            ExecuteRunPlayWithResult(game, rng);

            Assert.IsTrue(game.CurrentPlay.YardsGained >= 6, "Good gain");
        }

        [TestMethod]
        public void Matrix_RedZone20_HighSkills_Pass_Success()
        {
            var game = CreateGameInRedZoneForPass(80, Possession.Home);
            SetPassPlayerSkills(game, 88, 70);
            var rng = PassPlayScenarios.CompletedPassWithYAC(airYards: 15, yacFactor: 0.5);

            ExecutePassPlayWithResult(game, rng);

            Assert.IsTrue(((PassPlay)game.CurrentPlay).PassSegments[0].IsComplete, "Complete");
        }

        [TestMethod]
        public void Matrix_RedZone15_LowSkills_Run_Struggle()
        {
            var game = CreateGameInRedZone(85, Possession.Home);
            SetPlayerSkills(game, 55, 85);
            var rng = RunPlayScenarios.BadBlocking(yards: 3);

            ExecuteRunPlayWithResult(game, rng);

            Assert.IsTrue(game.CurrentPlay.YardsGained <= 4, "Limited gain");
        }

        [TestMethod]
        public void Matrix_RedZone10_HighSkills_Run_Touchdown()
        {
            var game = CreateGameInRedZone(90, Possession.Home);
            SetPlayerSkills(game, 90, 68);
            var rng = RunPlayScenarios.GoodBlocking(yards: 10);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(6, game.HomeScore, "TD scored");
        }

        [TestMethod]
        public void Matrix_RedZone10_HighSkills_Pass_Touchdown()
        {
            var game = CreateGameInRedZoneForPass(90, Possession.Home);
            SetPassPlayerSkills(game, 88, 70);
            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 10, immediateTackleYards: 0);

            ExecutePassPlayWithResult(game, rng);

            Assert.AreEqual(6, game.HomeScore, "TD scored");
        }

        [TestMethod]
        public void Matrix_RedZone5_HighSkills_Run_HighTDRate()
        {
            var game = CreateGameInRedZone(95, Possession.Home);
            SetPlayerSkills(game, 92, 70);
            var rng = RunPlayScenarios.GoodBlocking(yards: 5);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(6, game.HomeScore, "TD scored");
        }

        [TestMethod]
        public void Matrix_RedZone5_LowSkills_Run_Stopped()
        {
            var game = CreateGameInRedZone(95, Possession.Home);
            SetPlayerSkills(game, 50, 88);
            var rng = RunPlayScenarios.BadBlocking(yards: 2);

            ExecuteRunPlayWithResult(game, rng);

            Assert.AreEqual(0, game.HomeScore, "No TD");
        }

        #endregion

        #region Compressed Field Dynamics Tests

        [TestMethod]
        public void RedZone_CompressedField_DefenseAdvantage()
        {
            // Arrange - In red zone, defense has advantage with compressed field
            var game = CreateGameInRedZoneForPass(88, Possession.Home);
            SetPassPlayerSkills(game, 75, 80); // Even matchup favors defense

            var rng = PassPlayScenarios.IncompletePass(withPressure: true);

            // Act
            ExecutePassPlayWithResult(game, rng);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete,
                "Compressed field gives defense advantage");
        }

        [TestMethod]
        public void RedZone_PowerRunGame_MoreEffective()
        {
            // Arrange - Power run is more effective in red zone than midfield
            var game = CreateGameInRedZone(87, Possession.Home);
            SetPlayerSkills(game, 80, 75);

            var rng = RunPlayScenarios.GoodBlocking(yards: 6);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.IsTrue(runPlay.YardsGained >= 5,
                $"Power run effective in red zone (got {runPlay.YardsGained})");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameInRedZone(int fieldPosition, Possession possession)
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

        private Game CreateGameInRedZoneForPass(int fieldPosition, Possession possession)
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

        // Removed: Manual RNG creation methods replaced with RunPlayScenarios and PassPlayScenarios

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
