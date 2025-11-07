using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class PassPlayExecutionTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Basic Pass Play Execution Tests

        [TestMethod]
        public void PassPlay_CompletedPass_CreatesPassSegment()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 70, 70);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection=SUCCESS, Pressure=FAIL, receiver select, pass type, air yards
                __NextDouble = { [0] = 0.7, [1] = 0.5, [2] = 0.5, [3] = 0.6, [4] = 0.5, [5] = 0.5, [6] = 2.5 },
                __NextInt = { [0] = 8, [1] = 2 } // Air yards, YAC yards
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(1, passPlay.PassSegments.Count, "Should have exactly 1 pass segment");
            Assert.IsNotNull(passPlay.PassSegments[0].Passer, "Passer should be assigned");
            Assert.IsNotNull(passPlay.PassSegments[0].Receiver, "Receiver should be assigned");
            Assert.IsTrue(passPlay.ElapsedTime > 0, "Elapsed time should be set");
        }

        [TestMethod]
        public void PassPlay_IncompletePass_ZeroYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 50, 85); // Weak offense vs strong coverage

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection=SUCCESS, Pressure=FAIL, receiver, pass type, completion=FAIL, elapsed time
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.6, [4] = 0.9, [5] = 2.5 },
                __NextInt = { [0] = 8 } // Air yards (not used on incompletion)
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.AreEqual(0, passPlay.YardsGained, "Incomplete pass should have 0 yards");
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Should be marked as incomplete");
        }

        #endregion

        #region Sack Tests

        [TestMethod]
        public void PassPlay_Sack_NegativeYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var passPlay = (PassPlay)game.CurrentPlay;
            SetPlayerSkills(game, 40, 90); // Weak O-line vs strong pass rush

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection=FAIL, sack yards, elapsed time
                __NextDouble = { [0] = 0.3, [1] = 1.5 },
                __NextInt = { [0] = 7 } // Sack yards (7 yard loss)
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.YardsGained < 0, "Sack should result in negative yards");
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Sack should be marked as incomplete");
        }

        [TestMethod]
        public void PassPlay_SackAtOwnGoalLine_DoesNotExceedBoundary()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 3; // Very close to own goal line
            SetPlayerSkills(game, 40, 90);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection=FAIL, elapsed time
                __NextDouble = { [0] = 0.3, [1] = 1.5 },
                __NextInt = { [0] = 10 } // Would be 10 yard sack, but limited by field position
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - sack should be limited to 3 yards (can't go past own goal)
            Assert.IsTrue(game.CurrentPlay.YardsGained >= -3,
                $"Sack yards ({game.CurrentPlay.YardsGained}) should not exceed field position");
        }

        #endregion

        #region Pressure Impact Tests

        [TestMethod]
        public void PassPlay_WithPressure_AffectsCompletion()
        {
            // Arrange - two identical games except for pressure
            var game1 = CreateGameWithPassPlay();
            var game2 = CreateGameWithPassPlay();
            SetPlayerSkills(game1, 70, 70);
            SetPlayerSkills(game2, 70, 70);

            // No pressure scenario
            TestSeedableRandom rngNoPressure = new TestSeedableRandom
            {
                // Protection=SUCCESS, Pressure=FAIL, receiver, pass type, completion (60% base)
                __NextDouble = { [0] = 0.8, [1] = 0.8, [2] = 0.5, [3] = 0.6, [4] = 0.59, [5] = 0.8, [6] = 2.5 },
                __NextInt = { [0] = 10, [1] = 3 } // Air yards, YAC
            };

            // With pressure scenario (completion drops to ~40%)
            TestSeedableRandom rngPressure = new TestSeedableRandom
            {
                // Protection=SUCCESS, Pressure=SUCCESS, receiver, pass type, completion (60% - 20% = 40%)
                __NextDouble = { [0] = 0.8, [1] = 0.2, [2] = 0.5, [3] = 0.6, [4] = 0.59, [5] = 2.5 },
                __NextInt = { [0] = 10 } // Air yards
            };

            // Act
            var passNoPressure = new Pass(rngNoPressure);
            passNoPressure.Execute(game1);

            var passPressure = new Pass(rngPressure);
            passPressure.Execute(game2);

            // Assert
            var passPlay1 = (PassPlay)game1.CurrentPlay;
            var passPlay2 = (PassPlay)game2.CurrentPlay;

            Assert.IsTrue(passPlay1.PassSegments[0].IsComplete, "Should complete without pressure");
            Assert.IsFalse(passPlay2.PassSegments[0].IsComplete, "Should fail with pressure");
        }

        #endregion

        #region Pass Type Tests

        [TestMethod]
        public void PassPlay_ScreenPass_ShortAirYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type=SCREEN (< 0.15), completion, YAC, elapsed
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.10, [4] = 0.5, [5] = 0.3, [6] = 2.5 },
                __NextInt = { [0] = 1, [1] = 5 } // Air yards (-3 to +3 range), YAC
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(PassType.Screen, passPlay.PassSegments[0].Type, "Should be screen pass");
        }

        [TestMethod]
        public void PassPlay_DeepPass_LongAirYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type=DEEP (> 0.85), completion, YAC, elapsed
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.90, [4] = 0.5, [5] = 0.3, [6] = 2.5 },
                __NextInt = { [0] = 30, [1] = 5 } // Air yards (18-44 range), YAC
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(PassType.Deep, passPlay.PassSegments[0].Type, "Should be deep pass");
            Assert.IsTrue(passPlay.PassSegments[0].AirYards > 15, "Deep pass should have significant air yards");
        }

        #endregion

        #region Yards After Catch Tests

        [TestMethod]
        public void PassPlay_GoodYACCheck_AddsExtraYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);
            receiver.Speed = 90;
            receiver.Agility = 88;

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type, completion, YAC=SUCCESS, big play=FAIL, elapsed
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.6, [4] = 0.5, [5] = 0.3, [6] = 0.9, [7] = 2.5 },
                __NextInt = { [0] = 10, [1] = 8 } // Air yards, YAC yards
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch > 3, "Should have good YAC");
        }

        [TestMethod]
        public void PassPlay_BigPlayAfterCatch_SignificantYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);
            receiver.Speed = 95; // Fast receiver for big play potential
            receiver.Agility = 92;
            receiver.Rushing = 88;

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type, completion, YAC=SUCCESS, big play=SUCCESS, elapsed
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.6, [4] = 0.5, [5] = 0.3, [6] = 0.04, [7] = 2.5 },
                __NextInt = { [0] = 15, [1] = 8, [2] = 25 } // Air yards, base YAC, big play bonus
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.YardsGained > 30,
                $"Big play should result in significant yardage (got {passPlay.YardsGained})");
        }

        #endregion

        #region Field Boundary Tests

        [TestMethod]
        public void PassPlay_RespectsTouchdownBoundary_DoesNotExceed100Yards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 92; // Near the goal line
            SetPlayerSkills(game, 90, 50);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type=DEEP, completion, YAC=SUCCESS, big play, elapsed
                __NextDouble = { [0] = 0.9, [1] = 0.5, [2] = 0.5, [3] = 0.9, [4] = 0.5, [5] = 0.3, [6] = 0.9, [7] = 2.5 },
                __NextInt = { [0] = 40, [1] = 15 } // Would be 40 air yards + 15 YAC, but limited to 8 total
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var yardsToGoal = 100 - 92;
            Assert.IsTrue(game.CurrentPlay.YardsGained <= yardsToGoal,
                $"Yards gained ({game.CurrentPlay.YardsGained}) should not exceed yards to goal ({yardsToGoal})");
        }

        #endregion

        #region Receiver Selection Tests

        [TestMethod]
        public void PassPlay_SelectsReceiver_BasedOnCatchingAbility()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set one receiver to have very high catching (weighted selection should favor this one)
            var eliteReceiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);
            eliteReceiver.Catching = 95;

            // Set others to low catching
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player != eliteReceiver &&
                    (player.Position == Positions.WR || player.Position == Positions.TE || player.Position == Positions.RB))
                {
                    player.Catching = 40;
                }
            }

            SetPlayerSkills(game, 70, 70);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver (high value = elite receiver), pass type, completion, YAC, elapsed
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.9, [3] = 0.6, [4] = 0.5, [5] = 0.3, [6] = 2.5 },
                __NextInt = { [0] = 10, [1] = 5 }
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(eliteReceiver, passPlay.PassSegments[0].Receiver,
                "High weighted random should select elite receiver");
        }

        #endregion

        #region Elapsed Time Tests

        [TestMethod]
        public void PassPlay_NormalPass_ElapsedTime4To7Seconds()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var initialElapsedTime = game.CurrentPlay.ElapsedTime;
            SetPlayerSkills(game, 70, 70);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection, Pressure, receiver, pass type, completion, YAC, elapsed time = 4 + (1.5 * 3) = 8.5 clamped to max 7
                __NextDouble = { [0] = 0.8, [1] = 0.5, [2] = 0.5, [3] = 0.6, [4] = 0.5, [5] = 0.3, [6] = 1.5 },
                __NextInt = { [0] = 10, [1] = 5 }
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var timeAdded = game.CurrentPlay.ElapsedTime - initialElapsedTime;
            Assert.IsTrue(timeAdded >= 4.0 && timeAdded <= 7.0,
                $"Elapsed time ({timeAdded}) should be between 4 and 7 seconds");
        }

        [TestMethod]
        public void PassPlay_Sack_ElapsedTime2To4Seconds()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var initialElapsedTime = game.CurrentPlay.ElapsedTime;
            SetPlayerSkills(game, 40, 90);

            TestSeedableRandom rng = new TestSeedableRandom
            {
                // Protection=FAIL, elapsed time = 2 + (1.0 * 2) = 4.0
                __NextDouble = { [0] = 0.3, [1] = 1.0 },
                __NextInt = { [0] = 5 } // Sack yards
            };

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var timeAdded = game.CurrentPlay.ElapsedTime - initialElapsedTime;
            Assert.IsTrue(timeAdded >= 2.0 && timeAdded <= 4.0,
                $"Sack elapsed time ({timeAdded}) should be between 2 and 4 seconds");
        }

        #endregion

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
    }
}
