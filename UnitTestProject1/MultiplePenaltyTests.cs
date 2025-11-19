using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests for detecting multiple penalties on the same play.
    /// These tests verify the fix for the pre-snap penalty exclusion bug.
    /// </summary>
    [TestClass]
    public class MultiplePenaltyTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();

            // Create a pass play with proper formations
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Set offensive players (pass formation: 1 RB, 3 WR, 1 TE)
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

            // Set defensive players (nickel defense)
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
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        private Game CreateGameWithRunPlay()
        {
            var game = _testGame.GetGame();

            // Create a run play with proper formations
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
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
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        private void SetPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            // Set all offensive players to the same skill level
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = offenseSkill;
                player.Passing = offenseSkill;
                player.Catching = offenseSkill;
                player.Speed = offenseSkill;
                player.Agility = offenseSkill;
                player.Awareness = offenseSkill;
                player.Rushing = offenseSkill;
                player.Strength = offenseSkill;
            }

            // Set all defensive players to the same skill level
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = defenseSkill;
                player.Coverage = defenseSkill;
                player.Strength = defenseSkill;
                player.Speed = defenseSkill;
                player.Awareness = defenseSkill;
            }
        }

        #endregion

        #region Pass Play - Multiple Penalty Tests

        // All possible blocking penalties
        private static readonly PenaltyNames[] BlockingPenalties = new[]
        {
            PenaltyNames.OffensiveHolding,
            PenaltyNames.IllegalUseofHands,
            PenaltyNames.IllegalBlockAbovetheWaist,
            PenaltyNames.Clipping,
            PenaltyNames.ChopBlock,
            PenaltyNames.LowBlock,
            PenaltyNames.IllegalPeelback,
            PenaltyNames.IllegalCrackback
        };

        // All possible tackle penalties for receivers/ball carriers
        private static readonly PenaltyNames[] TacklePenalties = new[]
        {
            PenaltyNames.UnnecessaryRoughness,
            PenaltyNames.FaceMask15Yards,
            PenaltyNames.HorseCollarTackle,
            PenaltyNames.PersonalFoul,
            PenaltyNames.Tripping
        };

        [TestMethod]
        public void PassPlay_DetectsBothBlockingAndTacklePenalties()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithBlockingAndTacklePenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(2, passPlay.Penalties.Count,
                "Should detect both blocking penalty and tackle penalty");

            var penaltyTypes = passPlay.Penalties.Select(p => p.Name).ToList();
            Assert.IsTrue(penaltyTypes.Any(p => BlockingPenalties.Contains(p)),
                "Should have a blocking penalty");
            Assert.IsTrue(penaltyTypes.Any(p => TacklePenalties.Contains(p)),
                "Should have a tackle penalty");
        }

        [TestMethod]
        public void PassPlay_BlockingPenaltyAlone_DetectedCorrectly()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithBlockingPenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(1, passPlay.Penalties.Count, "Should have exactly one penalty");
            Assert.IsTrue(BlockingPenalties.Contains(passPlay.Penalties[0].Name),
                "Should be a blocking penalty");
        }

        [TestMethod]
        public void PassPlay_ReceiverTacklePenaltyAlone_DetectedCorrectly()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithReceiverTacklePenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(1, passPlay.Penalties.Count, "Should have exactly one penalty");
            Assert.IsTrue(TacklePenalties.Contains(passPlay.Penalties[0].Name),
                "Should be a tackle penalty");
        }

        #endregion

        #region Run Play - Multiple Penalty Tests

        [TestMethod]
        public void RunPlay_DetectsBothBlockingAndTacklePenalties()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.WithBlockingAndTacklePenalty(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(2, runPlay.Penalties.Count,
                "Should detect both blocking penalty and tackle penalty");

            var penaltyTypes = runPlay.Penalties.Select(p => p.Name).ToList();
            Assert.IsTrue(penaltyTypes.Any(p => BlockingPenalties.Contains(p)),
                "Should have a blocking penalty");
            Assert.IsTrue(penaltyTypes.Any(p => TacklePenalties.Contains(p)),
                "Should have a tackle penalty");
        }

        [TestMethod]
        public void RunPlay_BlockingPenaltyAlone_DetectedCorrectly()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.WithBlockingPenalty(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(1, runPlay.Penalties.Count, "Should have exactly one penalty");
            Assert.IsTrue(BlockingPenalties.Contains(runPlay.Penalties[0].Name),
                "Should be a blocking penalty");
        }

        [TestMethod]
        public void RunPlay_TacklePenaltyAlone_DetectedCorrectly()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.WithTacklePenalty(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(1, runPlay.Penalties.Count, "Should have exactly one penalty");
            Assert.IsTrue(TacklePenalties.Contains(runPlay.Penalties[0].Name),
                "Should be a tackle penalty");
        }

        #endregion

        #region Regression Tests

        [TestMethod]
        public void PassPlay_NoPenalties_WhenRandomValuesHigh()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(0, passPlay.Penalties.Count, "Should have no penalties");
        }

        [TestMethod]
        public void RunPlay_NoPenalties_WhenRandomValuesHigh()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.SimpleGain(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(0, runPlay.Penalties.Count, "Should have no penalties");
        }

        #endregion
    }
}
