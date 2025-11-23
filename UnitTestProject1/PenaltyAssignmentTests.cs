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
    /// Tests that verify penalties are correctly assigned to the appropriate team.
    /// This test suite specifically validates the fix for the "OffensiveHolding on defense" bug.
    /// </summary>
    [TestClass]
    public class PenaltyAssignmentTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Helper Methods

        private Game CreateGameWithPassPlay(Possession possession)
        {
            var game = _testGame.GetGame();

            var passPlay = new PassPlay
            {
                Possession = possession,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Determine offensive and defensive teams based on possession
            var offensiveTeam = possession == Possession.Home ? _teams.HomeTeam : _teams.VisitorTeam;
            var defensiveTeam = possession == Possession.Home ? _teams.VisitorTeam : _teams.HomeTeam;

            // Set offensive players
            passPlay.OffensePlayersOnField = new List<Player>
            {
                offensiveTeam.OffenseDepthChart.Chart[Positions.QB][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.RB][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.C][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.G][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.G][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.T][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.T][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.WR][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.WR][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.WR][2],
                offensiveTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            // Set defensive players
            passPlay.DefensePlayersOnField = new List<Player>
            {
                defensiveTeam.DefenseDepthChart.Chart[Positions.DE][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DE][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DT][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DT][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.LB][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.LB][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.CB][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.CB][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.S][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.S][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        private Game CreateGameWithRunPlay(Possession possession)
        {
            var game = _testGame.GetGame();

            var runPlay = new RunPlay
            {
                Possession = possession,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Determine offensive and defensive teams based on possession
            var offensiveTeam = possession == Possession.Home ? _teams.HomeTeam : _teams.VisitorTeam;
            var defensiveTeam = possession == Possession.Home ? _teams.VisitorTeam : _teams.HomeTeam;

            // Set offensive players
            runPlay.OffensePlayersOnField = new List<Player>
            {
                offensiveTeam.OffenseDepthChart.Chart[Positions.QB][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.RB][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.FB][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.C][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.G][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.G][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.T][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.T][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.WR][0],
                offensiveTeam.OffenseDepthChart.Chart[Positions.WR][1],
                offensiveTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            // Set defensive players
            runPlay.DefensePlayersOnField = new List<Player>
            {
                defensiveTeam.DefenseDepthChart.Chart[Positions.DE][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DE][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DT][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.DT][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.LB][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.LB][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.LB][2],
                defensiveTeam.DefenseDepthChart.Chart[Positions.CB][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.CB][1],
                defensiveTeam.DefenseDepthChart.Chart[Positions.S][0],
                defensiveTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = runPlay;
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        private void SetPlayerSkills(Game game, int offensiveSkill, int defensiveSkill)
        {
            var play = game.CurrentPlay;

            // Set all offensive player skills
            foreach (var player in play!.OffensePlayersOnField)
            {
                player.Speed = offensiveSkill;
                player.Strength = offensiveSkill;
                player.Catching = offensiveSkill;
                player.Blocking = offensiveSkill;
                player.Discipline = 50; // Medium discipline
            }

            // Set all defensive player skills
            foreach (var player in play.DefensePlayersOnField)
            {
                player.Speed = defensiveSkill;
                player.Strength = defensiveSkill;
                player.Tackling = defensiveSkill;
                player.Coverage = defensiveSkill;
                player.Discipline = 50; // Medium discipline
            }
        }

        #endregion

        #region Offensive Penalty Assignment Tests

        [TestMethod]
        public void PassPlay_OffensiveHolding_AlwaysCalledOnOffense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Home);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithBlockingPenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var offensiveHoldingPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.OffensiveHolding)
                .ToList();

            Assert.IsTrue(offensiveHoldingPenalties.Any(),
                "Should have an Offensive Holding penalty");

            foreach (var penalty in offensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Home, penalty.CalledOn,
                    $"Offensive Holding must be called on the offense (Home), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void PassPlay_OffensiveHolding_AlwaysCalledOnOffense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Away);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithBlockingPenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var offensiveHoldingPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.OffensiveHolding)
                .ToList();

            Assert.IsTrue(offensiveHoldingPenalties.Any(),
                "Should have an Offensive Holding penalty");

            foreach (var penalty in offensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Away, penalty.CalledOn,
                    $"Offensive Holding must be called on the offense (Away), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void RunPlay_OffensiveHolding_AlwaysCalledOnOffense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay(Possession.Home);
            var runPlay = (RunPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.WithBlockingPenalty(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.Penalties.Any(), "Should have at least one penalty");

            var offensiveHoldingPenalties = runPlay.Penalties
                .Where(p => p.Name == PenaltyNames.OffensiveHolding)
                .ToList();

            Assert.IsTrue(offensiveHoldingPenalties.Any(),
                "Should have an Offensive Holding penalty");

            foreach (var penalty in offensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Home, penalty.CalledOn,
                    $"Offensive Holding must be called on the offense (Home), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void RunPlay_OffensiveHolding_AlwaysCalledOnOffense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay(Possession.Away);
            var runPlay = (RunPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = RunPlayScenarios.WithBlockingPenalty(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.Penalties.Any(), "Should have at least one penalty");

            var offensiveHoldingPenalties = runPlay.Penalties
                .Where(p => p.Name == PenaltyNames.OffensiveHolding)
                .ToList();

            Assert.IsTrue(offensiveHoldingPenalties.Any(),
                "Should have an Offensive Holding penalty");

            foreach (var penalty in offensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Away, penalty.CalledOn,
                    $"Offensive Holding must be called on the offense (Away), but was called on {penalty.CalledOn}");
            }
        }

        #endregion

        #region Defensive Penalty Assignment Tests

        [TestMethod]
        public void PassPlay_DefensiveHolding_AlwaysCalledOnDefense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Home);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithDefensiveHoldingPenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var defensiveHoldingPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.DefensiveHolding)
                .ToList();

            Assert.IsTrue(defensiveHoldingPenalties.Any(),
                "Should have a Defensive Holding penalty");

            foreach (var penalty in defensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Away, penalty.CalledOn,
                    $"Defensive Holding must be called on the defense (Away), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void PassPlay_DefensiveHolding_AlwaysCalledOnDefense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Away);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithDefensiveHoldingPenalty(airYards: 10);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var defensiveHoldingPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.DefensiveHolding)
                .ToList();

            Assert.IsTrue(defensiveHoldingPenalties.Any(),
                "Should have a Defensive Holding penalty");

            foreach (var penalty in defensiveHoldingPenalties)
            {
                Assert.AreEqual(Possession.Home, penalty.CalledOn,
                    $"Defensive Holding must be called on the defense (Home), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void PassPlay_DefensivePassInterference_AlwaysCalledOnDefense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Home);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithDefensivePassInterferencePenalty(airYards: 20);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var dpiPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.DefensivePassInterference)
                .ToList();

            Assert.IsTrue(dpiPenalties.Any(),
                "Should have a Defensive Pass Interference penalty");

            foreach (var penalty in dpiPenalties)
            {
                Assert.AreEqual(Possession.Away, penalty.CalledOn,
                    $"Defensive Pass Interference must be called on the defense (Away), but was called on {penalty.CalledOn}");
            }
        }

        [TestMethod]
        public void PassPlay_DefensivePassInterference_AlwaysCalledOnDefense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay(Possession.Away);
            var passPlay = (PassPlay)game.CurrentPlay!;
            SetPlayerSkills(game, 70, 70);

            var rng = PassPlayScenarios.WithDefensivePassInterferencePenalty(airYards: 20);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.Penalties.Any(), "Should have at least one penalty");

            var dpiPenalties = passPlay.Penalties
                .Where(p => p.Name == PenaltyNames.DefensivePassInterference)
                .ToList();

            Assert.IsTrue(dpiPenalties.Any(),
                "Should have a Defensive Pass Interference penalty");

            foreach (var penalty in dpiPenalties)
            {
                Assert.AreEqual(Possession.Home, penalty.CalledOn,
                    $"Defensive Pass Interference must be called on the defense (Home), but was called on {penalty.CalledOn}");
            }
        }

        #endregion
    }
}
