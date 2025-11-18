using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;
using System.Collections.Generic;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests for dead ball (pre-snap) penalties that prevent play execution.
    /// Verifies that GameFlow correctly aborts plays when dead ball fouls occur.
    /// </summary>
    [TestClass]
    public class DeadBallPenaltyTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            game.Possession = Possession.Home;

            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Set offensive players
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

        #endregion

        #region False Start Tests

        [TestMethod]
        public void FalseStart_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 30;

            // Add false start penalty before play executes
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FalseStart,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            var initialFieldPosition = game.FieldPosition;

            // Act - Use GameFlow to process the play
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);

            // Trigger pre-snap which should detect dead ball foul
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "No yards should be gained - play was aborted");
            Assert.AreEqual(25, game.FieldPosition, "Should be moved back 5 yards (30 - 5)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should still be first down");
            Assert.AreEqual(15, game.YardsToGo, "Should be 1st and 15 (10 + 5)");
        }

        [TestMethod]
        public void FalseStart_MultipleOccurrences_StillPreventsPlay()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 50;
            game.YardsToGo = 5;
            game.CurrentDown = Downs.Second;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 50;

            // Add false start
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FalseStart,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(45, game.FieldPosition, "Moved back 5 yards (50 - 5)");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Still second down");
            Assert.AreEqual(10, game.YardsToGo, "Now 2nd and 10 (5 + 5)");
        }

        #endregion

        #region Encroachment Tests

        [TestMethod]
        public void Encroachment_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 40;
            game.YardsToGo = 7;
            game.CurrentDown = Downs.Second;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 40;

            // Add encroachment penalty (defensive)
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.Encroachment,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(45, game.FieldPosition, "Moved forward 5 yards (40 + 5)");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Still second down");
            Assert.AreEqual(2, game.YardsToGo, "Now 2nd and 2 (7 - 5)");
        }

        #endregion

        #region Delay of Game Tests

        [TestMethod]
        public void DelayOfGame_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 20;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Third;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 20;

            // Add delay of game penalty (offensive)
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DelayofGame,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(15, game.FieldPosition, "Moved back 5 yards (20 - 5)");
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Still third down");
            Assert.AreEqual(15, game.YardsToGo, "Now 3rd and 15 (10 + 5)");
        }

        [TestMethod]
        public void DefensiveDelayOfGame_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 35;
            game.YardsToGo = 5;
            game.CurrentDown = Downs.Third;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 35;

            // Add defensive delay of game penalty
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DefensiveDelayofGame,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(40, game.FieldPosition, "Moved forward 5 yards (35 + 5)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down (gained needed yards)");
            Assert.AreEqual(10, game.YardsToGo, "Now 1st and 10");
        }

        #endregion

        #region Too Many Players Tests

        [TestMethod]
        public void Offensive12OnField_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 50;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 50;

            // Add offensive 12 on field penalty
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.Offensive12OnField,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(45, game.FieldPosition, "Moved back 5 yards (50 - 5)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Still first down");
            Assert.AreEqual(15, game.YardsToGo, "Now 1st and 15");
        }

        [TestMethod]
        public void Defensive12OnField_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Second;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 30;

            // Add defensive 12 on field penalty
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.Defensive12OnField,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(35, game.FieldPosition, "Moved forward 5 yards (30 + 5)");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Still second down");
            Assert.AreEqual(5, game.YardsToGo, "Now 2nd and 5 (10 - 5)");
        }

        #endregion

        #region Illegal Substitution Tests

        [TestMethod]
        public void IllegalSubstitution_PreventsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 45;
            game.YardsToGo = 3;
            game.CurrentDown = Downs.Third;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 45;

            // Add illegal substitution penalty
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.IllegalSubstitution,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(50, game.FieldPosition, "Moved forward 5 yards (45 + 5) - converts first down");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down (gained > needed)");
            Assert.AreEqual(10, game.YardsToGo, "Now 1st and 10");
        }

        #endregion

        #region Offsetting Dead Ball Penalties Tests

        [TestMethod]
        public void OffsettingDeadBallPenalties_PreventPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 40;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Second;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 40;

            // Add offsetting dead ball penalties
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FalseStart,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                },
                new Penalty
                {
                    Name = PenaltyNames.Encroachment,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(40, game.FieldPosition, "Field position unchanged - offsetting penalties");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Replay second down");
            Assert.AreEqual(10, game.YardsToGo, "Still 2nd and 10");
        }

        #endregion

        #region Non-Dead Ball Penalty Tests (Play Should Execute)

        [TestMethod]
        public void OffensiveHolding_AllowsPlayExecution()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 30;

            // Add offensive holding (occurs DURING play, not dead ball)
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.OffensiveHolding,
                    Yards = 10,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.During,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            // Act
            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 15);
            var gameFlow = new GameFlow(game, rng);

            // DoPreSnap should NOT abort the play (holding is not a dead ball foul)
            gameFlow.DoPreSnap();

            // Assert - Since holding is not a dead ball foul, snap should have occurred
            // The snap sets GoodSnap property
            Assert.IsTrue(passPlay.GoodSnap, "Snap should have occurred - holding is not dead ball foul");
        }

        [TestMethod]
        public void DefensiveOffside_AllowsPlayExecution_FreePlay()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 25;

            // Add defensive offsides (NOT a dead ball foul - play continues as "free play")
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DefensiveOffside,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = PassPlayScenarios.CompletedPassImmediateTackle(airYards: 20);
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert - Offsides is not a dead ball foul, so play continues
            Assert.IsTrue(passPlay.GoodSnap, "Snap should occur - offsides allows free play");
        }

        #endregion

        #region Edge Cases - Goal Line

        [TestMethod]
        public void FalseStart_NearOwnGoalLine_StopsAtOne()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 2; // Very close to own goal
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 2;

            // Add false start
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FalseStart,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.OffensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert - Can't go into own end zone on dead ball penalty
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(1, game.FieldPosition, "Should stop at 1-yard line (can't safety on dead ball)");
        }

        [TestMethod]
        public void Encroachment_NearOpponentGoalLine_StopsAt99()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 97; // Very close to opponent goal
            game.YardsToGo = 3;
            game.CurrentDown = Downs.Third;

            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.StartFieldPosition = 97;

            // Add encroachment (defensive - helps offense)
            passPlay.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.Encroachment,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    OccuredWhen = PenaltyOccuredWhen.Before,
                    Accepted = true,
                    CommittedBy = passPlay.DefensePlayersOnField[0]
                }
            };

            // Act
            var rng = new TestFluentSeedableRandom();
            var gameFlow = new GameFlow(game, rng);
            gameFlow.DoPreSnap();

            // Assert - Can't score on dead ball penalty
            Assert.AreEqual(0, passPlay.YardsGained, "Play should not execute");
            Assert.AreEqual(99, game.FieldPosition, "Should stop at 99-yard line (can't TD on dead ball)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down (penalty gave > needed yards)");
        }

        #endregion
    }
}
