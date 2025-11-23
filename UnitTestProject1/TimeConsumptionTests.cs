using System.Collections.Generic;
using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests for realistic game clock time consumption and stoppage logic.
    /// Verifies that plays consume appropriate execution time and that the clock
    /// stops/runs according to NFL rules.
    /// </summary>
    [TestClass]
    public class TimeConsumptionTests
    {
        private readonly TestGame _testGame = new TestGame();

        #region Pass Play Time Tests

        [TestMethod]
        public void PassPlay_Incomplete_StopsClock()
        {
            // Arrange
            var game = _testGame.GetGame();
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 50,
                YardsGained = 0,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.QB, Passing = 85, Speed = 75, Strength = 70, Agility = 75, Awareness = 90 },
                    new Player { Position = Positions.WR, Catching = 85, Speed = 92, Agility = 90, Awareness = 80 }
                },
                DefensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.CB, Coverage = 80, Speed = 88, Agility = 85, Awareness = 80 }
                }
            };
            game.CurrentPlay = passPlay;
            
            var rng = PassPlayScenarios.IncompletePass(withPressure: false);
            var pass = new Pass(rng);

            // Act
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.ClockStopped, "Clock should stop on incomplete pass");
            Assert.IsGreaterThan(0, passPlay.ElapsedTime, "Incomplete pass should still consume execution time");
        }

        [TestMethod]
        public void PassPlay_Interception_StopsClock()
        {
            // Arrange
            var game = _testGame.GetGame();
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 50,
                YardsGained = 0,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.QB, Passing = 85, Speed = 75, Strength = 70, Agility = 75, Awareness = 90 },
                    new Player { Position = Positions.WR, Catching = 85, Speed = 92, Agility = 90, Awareness = 80 }
                },
                DefensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.CB, Coverage = 90, Speed = 92, Agility = 90, Awareness = 85, Catching = 75 }
                }
            };
            game.CurrentPlay = passPlay;
            
            var rng = PassPlayScenarios.Interception(returnYards: 15, withFumble: false);
            var pass = new Pass(rng);

            // Act
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.ClockStopped, "Clock should stop on interception");
        }

        #endregion

        #region Special Teams Time Tests

        [TestMethod]
        public void Kickoff_StopsClock()
        {
            // Arrange
            var game = _testGame.GetGame(); // Starts with kickoff
            var play = (KickoffPlay)game.CurrentPlay!;
            
            var rng = KickoffPlayScenarios.NormalReturn(0.5, 0.5);
            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);

            // Assert
            Assert.IsTrue(play.ClockStopped, "Clock should stop after kickoff");
            Assert.IsGreaterThan(0, play.ElapsedTime, "Kickoff should consume time");
        }

        [TestMethod]
        public void Punt_StopsClock()
        {
            // Arrange
            var game = _testGame.GetGame();
            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartFieldPosition = 0,
                YardsGained = 0,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.P, Kicking = 75, Speed = 50, Strength = 50, Agility = 50, Catching = 40 },
                    new Player { Position = Positions.LS, Blocking = 75, Speed = 50, Strength = 60 }
                },
                DefensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.CB, Speed = 85, Agility = 80, Catching = 75, Tackling = 60 }
                }
            };
            game.CurrentPlay = puntPlay;
            
            var rng = PuntPlayScenarios.NormalReturn(0.5, 0.5);
            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(puntPlay.ClockStopped, "Clock should stop after punt");
            Assert.IsGreaterThan(0, puntPlay.ElapsedTime, "Punt should consume time");
        }

        [TestMethod]
        public void FieldGoal_StopsClock()
        {
            // Arrange
            var game = _testGame.GetGame();
            var fgPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartFieldPosition = 0,
                YardsGained = 0,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.K, Kicking = 85, Speed = 50, Strength = 60, Agility = 60, Awareness = 75 },
                    new Player { Position = Positions.QB, Passing = 70, Catching = 70, Speed = 65, Awareness = 80 },
                    new Player { Position = Positions.LS, Blocking = 75, Speed = 50, Strength = 60 }
                },
                DefensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.DE, Speed = 75, Strength = 80, Tackling = 70, Awareness = 65 }
                }
            };
            game.CurrentPlay = fgPlay;
            game.FieldPosition = 70;
            
            var rng = FieldGoalPlayScenarios.MediumFieldGoalMade(0.5);
            var fg = new FieldGoal(rng);

            // Act
            fg.Execute(game);

            // Assert
            Assert.IsTrue(fgPlay.ClockStopped, "Clock should stop after field goal");
            Assert.IsGreaterThan(0, fgPlay.ElapsedTime, "Field goal should consume time");
        }

        #endregion
    }
}
