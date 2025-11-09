using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.PlayResults;
using StateLibrary.Plays;
using System.Collections.Generic;
using System.Linq;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PuntPlayExecutionTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Bad Snap Tests

        [TestMethod]
        public void Punt_BadSnap_PunterRecovers_LosesYards()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 30;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01)  // Bad snap occurs (< 2.2% with 70 blocking)
                .NextDouble(0.3)   // Base loss calculation
                .NextDouble(0.5)   // Random factor
                .NextDouble(0.7);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsFalse(play.GoodSnap, "Should be marked as bad snap");
            Assert.IsTrue(play.YardsGained < 0, "Should lose yards on bad snap");
            Assert.IsTrue(play.YardsGained >= -30, "Can't lose more than current field position");
        }

        [TestMethod]
        public void Punt_BadSnapAtOwnGoalLine_ResultsInSafety()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 5;  // Very close to own goal line
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 0;
            game.AwayScore = 0;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01)  // Bad snap occurs (< 2.2% with 70 blocking)
                .NextDouble(0.9)   // Large loss (near max)
                .NextDouble(0.8)   // Random factor pushes it over
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsFalse(play.GoodSnap, "Should be bad snap");
            Assert.AreEqual(-5, play.YardsGained, "Should lose exactly 5 yards (to goal line)");
            Assert.IsTrue(play.IsSafety, "Should be marked as safety");
            Assert.AreEqual(2, game.AwayScore, "Defense (Away) should get 2 points for safety");
            Assert.AreEqual(0, game.HomeScore, "Home score should not change");
        }

        [TestMethod]
        public void Punt_GoodSnap_ContinuesToPunt()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 30;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.96)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.5)   // Punt distance
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.8)   // Not downed
                .NextDouble(0.8)   // Not fair catch
                .NextDouble(0.95)  // No muff
                .NextDouble(0.5)   // Return yards factors...
                .NextDouble(0.5)
                .NextDouble(0.5)
                .NextDouble(0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.GoodSnap, "Should be good snap");
            Assert.IsFalse(play.Blocked, "Should not be blocked");
        }

        #endregion

        #region Blocked Punt Tests

        [TestMethod]
        public void Punt_BlockedAndRecoveredByDefense_ChangePossession()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED! (Next(2) returns 1)
                .NextDouble(0.6)   // Defense recovers (> 50%)
                .NextDouble(0.3)   // Recovery yards
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Punt should be blocked");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsNotNull(play.RecoveredBy, "Someone should recover");
            Assert.IsTrue(play.YardsGained >= 0, "Recovery should gain yards for defense");
            Assert.IsTrue(play.YardsGained <= 15, "Recovery limited to 0-15 yards");
        }

        [TestMethod]
        public void Punt_BlockedAndRecoveredByOffense_LosesYards()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED! (Next(2) returns 1)
                .NextDouble(0.3)   // Offense recovers (< 50%)
                .NextDouble(0.7)   // Loss calculation
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Blocked, "Punt should be blocked");
            Assert.IsNotNull(play.RecoveredBy, "Offense should recover");
            Assert.IsTrue(play.YardsGained < 0, "Should lose yards");
            Assert.IsTrue(play.YardsGained >= -10, "Loss limited to -5 to -10 yards");
        }

        [TestMethod]
        public void Punt_BlockedRecoveredForTouchdown_ScoresPoints()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 95;  // Near opponent goal line
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 14;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED! (Next(2) returns 1)
                .NextDouble(0.6)   // Defense recovers
                .NextDouble(0.9)   // Big return (10+ yards to TD)
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Punt should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be a touchdown");
            Assert.AreEqual(20, game.AwayScore, "Away team should score TD (14 + 6)");
            Assert.AreEqual(7, game.HomeScore, "Home score should not change");
        }

        #endregion

        #region Touchback Tests

        [TestMethod]
        public void Punt_IntoEndZone_ResultsInTouchback()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.95)  // Punt distance
                .NextDouble(0.9);  // Hang time random

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Touchback, "Should be a touchback");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsFalse(play.Blocked, "Should not be blocked");
        }

        #endregion

        #region Out of Bounds Tests

        [TestMethod]
        public void Punt_OutOfBounds_NoReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.6)   // Punt distance
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.1);  // OUT OF BOUNDS!

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsFalse(play.Blocked, "Should not be blocked");
            Assert.IsFalse(play.Touchback, "Should not be touchback");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsTrue(play.PuntDistance > 0, "Should have punt distance");
            Assert.AreEqual(0, play.ReturnSegments.Count, "Should have no return");
        }

        #endregion

        #region Punt Downed Tests

        [TestMethod]
        public void Punt_DownedByPuntingTeam_NoReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.85)  // Punt distance
                .NextDouble(0.7)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.2);  // DOWNED!

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Downed, "Punt should be downed");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsTrue(play.DownedAtYardLine > 0, "Should have downed location");
            Assert.AreEqual(0, play.ReturnSegments.Count, "Should have no return");
        }

        [TestMethod]
        public void Punt_DownedNearGoalLine_GoodFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 50;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.35)  // 45-yard punt (lands at ~95 yard line)
                .NextDouble(0.9)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.1);  // DOWNED! (high probability near goal line)

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Downed, "Should be downed");
            Assert.IsTrue(play.DownedAtYardLine > 90, "Should be downed deep in opponent territory");
        }

        #endregion

        #region Fair Catch Tests

        [TestMethod]
        public void Punt_FairCatch_NoReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.65)  // Punt distance
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.9)   // Not out of bounds
                .NextDouble(0.9)   // Not downed
                .NextDouble(0.2);  // FAIR CATCH! (55% chance with hang time + field position)

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.FairCatch, "Should be fair catch");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(0, play.ReturnSegments.Count, "Should have no return");
        }

        [TestMethod]
        public void Punt_FairCatchDeepInOwnTerritory_HighProbability()
        {
            // Arrange - Punt lands at own 8 yard line (field position 92 from punter's perspective)
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.63)  // 52-yard punt (lands at own 8, field position 92)
                .NextDouble(0.9)   // Hang time random (good hang time ~4.8s)
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.6)   // Not downed (0.6 >= 50% threshold at spot 92)
                .NextDouble(0.15); // FAIR CATCH! (60% chance: deep + good hang time)

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.FairCatch, "Should be fair catch deep in own territory");
        }

        #endregion

        #region Muffed Catch Tests

        [TestMethod]
        public void Punt_MuffedCatch_ReceivingTeamRecovers()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.6)   // Punt distance
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.9)   // Not downed
                .NextDouble(0.9)   // Not fair catch
                .NextDouble(0.02)  // MUFFED!
                .NextDouble(0.4)   // Receiving team recovers (< 60%)
                .NextDouble(0.3)   // Recovery yards
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.MuffedCatch, "Should be muffed catch");
            Assert.IsTrue(play.PossessionChange, "Possession should still change (receiving team recovered)");
            Assert.IsNotNull(play.MuffedBy, "Should record who muffed");
            Assert.IsNotNull(play.RecoveredBy, "Should record who recovered");
        }

        [TestMethod]
        public void Punt_MuffedCatch_PuntingTeamRecovers()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 35;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.6)   // Punt distance
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.9)   // Not downed
                .NextDouble(0.9)   // Not fair catch
                .NextDouble(0.03)  // MUFFED!
                .NextDouble(0.7)   // Punting team recovers (>= 60%)
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.MuffedCatch, "Should be muffed catch");
            Assert.IsFalse(play.PossessionChange, "Possession should NOT change (punting team recovered)");
            Assert.IsNotNull(play.MuffedBy, "Should record who muffed");
            Assert.IsNotNull(play.RecoveredBy, "Should record who recovered");
        }

        #endregion

        #region Normal Punt Return Tests

        [TestMethod]
        public void Punt_NormalReturn_AdvancesField()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.65)  // Punt distance
                .NextDouble(0.6)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.9)   // Not downed
                .NextDouble(0.9)   // Not fair catch
                .NextDouble(0.95)  // No muff
                .NextDouble(0.5)   // Return yards (includes randomness)
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsFalse(play.Blocked, "Should not be blocked");
            Assert.IsFalse(play.FairCatch, "Should not be fair catch");
            Assert.IsFalse(play.MuffedCatch, "Should not be muffed");
            Assert.AreEqual(1, play.ReturnSegments.Count, "Should have one return segment");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsNotNull(play.InitialReturner, "Should have a returner");
        }

        [TestMethod]
        public void Punt_ReturnForTouchdown_ScoresPoints()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 60;  // Own 40-yard line
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 14;
            game.AwayScore = 10;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.05)  // Short punt ~38 yards (lands at ~98)
                .NextDouble(0.3)   // Short hang time (poor coverage)
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.9)   // Not downed
                .NextDouble(0.9)   // Not fair catch
                .NextDouble(0.95)  // No muff
                .NextDouble(0.95)  // Great return ~20+ yards for TD
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be touchdown on return");
            Assert.AreEqual(16, game.AwayScore, "Away team should score TD (10 + 6)");
            Assert.AreEqual(14, game.HomeScore, "Home score should not change");
            Assert.AreEqual(1, play.ReturnSegments.Count, "Should have return segment");
        }

        [TestMethod]
        public void Punt_NoReturner_TreatedAsDowned()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;
            var play = (PuntPlay)game.CurrentPlay;

            // Remove all potential returners from defense
            play.DefensePlayersOnField.Clear();

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.6)   // Punt distance (includes randomness)
                .NextDouble(0.5)   // Hang time random
                .NextDouble(0.8)   // Not out of bounds
                .NextDouble(0.9)   // Not downed (but will be treated as downed due to no returner)
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(play.Downed || play.ReturnSegments.Count == 0, "Should be treated as downed with no returner");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        #endregion

        #region Field Position Validation Tests

        [TestMethod]
        public void Punt_FromOwnGoalLine_CannotGoNegative()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 2;  // Very close to own goal

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01)  // Bad snap occurs (< 2.2% with 70 blocking)
                .NextDouble(0.95)  // Large loss attempt
                .NextDouble(0.9)   // Random factor
                .NextDouble(0.5);  // Elapsed time

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.YardsGained >= -2, "Cannot lose more than current field position");
        }

        [TestMethod]
        public void Punt_NearOpponentGoalLine_CannotExceed100()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.99)  // Punt distance (very long)
                .NextDouble(0.5);  // Hang time random

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            // Should be touchback since punt would exceed 100
            Assert.IsTrue(play.Touchback || play.PuntDistance <= 55, "Punt distance should be clamped or result in touchback");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void Punt_MultipleScenarios_AllExecuteWithoutError()
        {
            // This test ensures all punt paths execute without exceptions

            // Test 1: Bad snap (no block check needed - returns early)
            ExecutePuntScenario(CreateGameWithPuntPlay(), new TestFluentSeedableRandom()
                .NextDouble(0.01).NextDouble(0.5).NextDouble(0.5).NextDouble(0.5));

            // Test 2: Blocked punt
            ExecutePuntScenario(CreateGameWithPuntPlay(), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(1).NextDouble(0.5).NextDouble(0.5).NextDouble(0.5));

            // Test 3: Touchback
            ExecutePuntScenario(CreateGameWithPuntPlay(), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.95).NextDouble(0.5).NextDouble(0.8).NextDouble(0.5));

            // Test 4: Fair catch
            ExecutePuntScenario(CreateGameWithPuntPlay(), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.6).NextDouble(0.5).NextDouble(0.8)
                .NextDouble(0.9).NextDouble(0.1).NextDouble(0.5));

            // Test 5: Normal return
            ExecutePuntScenario(CreateGameWithPuntPlay(), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.6).NextDouble(0.5).NextDouble(0.8)
                .NextDouble(0.9).NextDouble(0.9).NextDouble(0.95).NextDouble(0.5).NextDouble(0.5));
        }

        private void ExecutePuntScenario(Game game, TestFluentSeedableRandom rng)
        {
            var punt = new Punt(rng);
            punt.Execute(game);
            // Just verify it doesn't throw
            Assert.IsNotNull(game.CurrentPlay);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPuntPlay()
        {
            var game = _testGame.GetGame();

            // Create a punt play
            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartFieldPosition = 0,
                YardsGained = 0,
                OffensePlayersOnField = new List<Player>(),
                DefensePlayersOnField = new List<Player>()
            };

            // Add punter
            puntPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.P,
                LastName = "Punter",
                Kicking = 65,
                Speed = 50,
                Strength = 50,
                Agility = 50,
                Catching = 40
            });

            // Add long snapper
            puntPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.LS,
                LastName = "Snapper",
                Blocking = 70,
                Speed = 50,
                Strength = 60
            });

            // Add potential returners
            for (int i = 0; i < 3; i++)
            {
                puntPlay.DefensePlayersOnField.Add(new Player
                {
                    Position = Positions.CB,
                    LastName = $"Returner{i}",
                    Speed = 80,
                    Agility = 75,
                    Catching = 70,
                    Tackling = 60
                });
            }

            // Add coverage team
            for (int i = 0; i < 3; i++)
            {
                puntPlay.OffensePlayersOnField.Add(new Player
                {
                    Position = Positions.S,
                    LastName = $"Gunner{i}",
                    Speed = 85,
                    Tackling = 70
                });
            }

            game.CurrentPlay = puntPlay;
            game.FieldPosition = 40;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Fourth;
            game.HomeScore = 0;
            game.AwayScore = 0;

            return game;
        }

        #endregion
    }
}
