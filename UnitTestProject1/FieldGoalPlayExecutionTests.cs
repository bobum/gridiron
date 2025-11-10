using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using StateLibrary.Tests;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class FieldGoalPlayExecutionTests
    {
        private TestGame _testGame;

        [TestInitialize]
        public void Setup()
        {
            _testGame = new TestGame();
        }

        #region Extra Point Tests

        [TestMethod]
        public void ExtraPoint_Made_Scores1Point()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: true);
            game.FieldPosition = 97; // 3-yard line (20-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 6; // Just scored TD
            game.AwayScore = 0;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.5)   // Make (98% for PAT)
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsGood, "Extra point should be good");
            Assert.IsTrue(play.IsExtraPoint, "Should be marked as extra point");
            Assert.AreEqual(7, game.HomeScore, "Home score should be 7 (6 + 1)");
            Assert.AreEqual(20, play.AttemptDistance, "PAT distance should be 20 yards");
        }

        [TestMethod]
        public void ExtraPoint_Missed_NoScore()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: true);
            game.FieldPosition = 97; // 3-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 6;
            game.AwayScore = 0;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.99)  // Miss (unlikely but possible)
                .NextDouble(0.5)   // Miss direction
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Extra point should be missed");
            Assert.AreEqual(6, game.HomeScore, "Home score should stay at 6");
        }

        #endregion

        #region Short Field Goal Tests (< 30 yards)

        [TestMethod]
        public void FieldGoal_Short_Made()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 90; // 10-yard line (27-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 0;
            game.AwayScore = 0;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.5)   // Make (high probability for short FG)
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsGood, "Short FG should be good");
            Assert.IsFalse(play.IsExtraPoint, "Should be field goal, not PAT");
            Assert.AreEqual(3, game.HomeScore, "Home score should be 3");
            Assert.AreEqual(27, play.AttemptDistance, "Should be 27-yard attempt");
        }

        #endregion

        #region Medium Field Goal Tests (30-50 yards)

        [TestMethod]
        public void FieldGoal_Medium_Made()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70; // 30-yard line (47-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            game.HomeScore = 7;
            game.AwayScore = 7;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.4)   // Make (good kicker at 47 yards)
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsGood, "Medium FG should be good");
            Assert.AreEqual(10, game.AwayScore, "Away score should be 10 (7 + 3)");
            Assert.AreEqual(47, play.AttemptDistance, "Should be 47-yard attempt");
        }

        [TestMethod]
        public void FieldGoal_Medium_Missed()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70; // 30-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 14;
            game.AwayScore = 10;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.95)  // Miss
                .NextDouble(0.3)   // Wide right
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(14, game.HomeScore, "Home score should stay at 14");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        #endregion

        #region Long Field Goal Tests (50-60 yards)

        [TestMethod]
        public void FieldGoal_Long_Made()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 50; // Midfield (67-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            // Use excellent kicker
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85; // Excellent kicker

            game.HomeScore = 0;
            game.AwayScore = 3;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.2)   // Make (excellent kicker at 67 yards ~40% chance)
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsGood, "Long FG by excellent kicker should be good");
            Assert.AreEqual(3, game.HomeScore, "Home score should be 3");
            Assert.AreEqual(67, play.AttemptDistance, "Should be 67-yard attempt");
        }

        [TestMethod]
        public void FieldGoal_Long_Missed()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 50; // Midfield
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            game.HomeScore = 10;
            game.AwayScore = 7;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.8)   // Miss (likely at 67 yards)
                .NextDouble(0.85)  // Short
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Long FG should be missed");
            Assert.AreEqual(7, game.AwayScore, "Away score should stay at 7");
        }

        #endregion

        #region Bad Snap Tests

        [TestMethod]
        public void FieldGoal_BadSnap_LosesYards()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01)  // Bad snap occurs
                .NextDouble(0.5)   // Base loss
                .NextDouble(0.5)   // Random factor
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsFalse(play.GoodSnap, "Should be marked as bad snap");
            Assert.IsFalse(play.IsGood, "Kick should not be good");
            Assert.IsTrue(play.YardsGained < 0, "Should lose yards on bad snap");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void FieldGoal_BadSnapAtGoalLine_ResultsInSafety()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 5; // Very close to own goal
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 0;
            game.AwayScore = 0;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01)  // Bad snap occurs
                .NextDouble(0.9)   // Large loss
                .NextDouble(0.9)   // Random factor pushes into end zone
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsSafety, "Should be safety");
            Assert.AreEqual(2, game.AwayScore, "Away team should get 2 points");
            Assert.AreEqual(0, game.FieldPosition, "Field position should be 0");
        }

        #endregion

        #region Blocked Kick Tests

        [TestMethod]
        public void FieldGoal_Blocked_NoReturn()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED!
                .NextDouble(0.6)   // Offense recovers/ball dead (>= 40%)
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Kick should be blocked");
            Assert.IsFalse(play.IsGood, "Blocked kick is not good");
            Assert.IsNotNull(play.BlockedBy, "Should track who blocked");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void FieldGoal_Blocked_DefenseRecoversAndReturns()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED!
                .NextDouble(0.3)   // Defense recovers (< 40%)
                .NextDouble(0.5)   // Base return
                .NextDouble(0.5)   // Random factor
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsNotNull(play.BlockedBy, "Should track blocker");
        }

        [TestMethod]
        public void FieldGoal_Blocked_ReturnedForTouchdown()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 95; // Near opponent goal
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 3;
            game.AwayScore = 7;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED!
                .NextDouble(0.3)   // Defense recovers
                .NextDouble(0.8)   // Good return
                .NextDouble(0.8)   // Random factor for TD
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be TD on return");
            Assert.AreEqual(13, game.AwayScore, "Away team should score TD (7 + 6)");
        }

        [TestMethod]
        public void FieldGoal_Blocked_ReturnedIntoEndZone_DefensiveTD()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 8; // Close to own end zone
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 10;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(1)        // BLOCKED!
                .NextDouble(0.3)   // Defense recovers
                .NextDouble(0.05)  // Ball bounces backward
                .NextDouble(0.1)   // Into end zone
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be TD in end zone");
            Assert.AreEqual(16, game.AwayScore, "Away team should score TD (10 + 6)");
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void FieldGoal_NoKickerOnField_UsesBackup()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Remove kicker
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.K);

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.5)   // Attempt
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsNotNull(game.CurrentPlay, "Play should complete");
            // Backup (punter) will have lower kicking skill, so result may vary
        }

        [TestMethod]
        public void FieldGoal_From1YardLine_HandledCorrectly()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 1; // Own 1-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.99)  // Likely miss from 116 yards
                .NextDouble(0.5)   // Miss direction
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            var play2 = (FieldGoalPlay)game.CurrentPlay;
            Assert.AreEqual(116, play2.AttemptDistance, "Should be 116-yard attempt");
            Assert.IsFalse(play2.IsGood, "116-yard kick should miss");
        }

        [TestMethod]
        public void FieldGoal_ExcellentKicker_HigherSuccessRate()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 60; // 40-yard line (57-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Excellent kicker (85 skill)
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.4)   // Should make with excellent kicker
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsGood, "Excellent kicker should make 57-yarder");
        }

        [TestMethod]
        public void FieldGoal_PoorKicker_LowerSuccessRate()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 80; // 20-yard line (37-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Poor kicker (30 skill)
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 30;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.75)  // More likely to miss with poor kicker (0.75 > 0.73)
                .NextDouble(0.5)   // Miss direction
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Poor kicker should miss more often");
        }

        [TestMethod]
        public void FieldGoal_MissedFieldPosition_OpponentGetsAtSpot()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70; // 30-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.95)  // Miss
                .NextDouble(0.5)   // Miss direction
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(70, play.EndFieldPosition, "Defense gets ball at spot of kick");
        }

        [TestMethod]
        public void FieldGoal_MissedFromRedZone_OpponentGetsAt20()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 95; // 5-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextInt(0)        // No block
                .NextDouble(0.99)  // Miss
                .NextDouble(0.5)   // Miss direction
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(80, play.EndFieldPosition, "Defense gets ball at their 20");
            Assert.AreEqual(80, game.FieldPosition, "Game field position should be 80");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void FieldGoal_MultipleScenarios_AllExecuteWithoutError()
        {
            // Test various scenarios to ensure no exceptions

            // Test 1: Made field goal
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.5).NextDouble(0.5));

            // Test 2: Missed field goal
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.95).NextDouble(0.5).NextDouble(0.5));

            // Test 3: Made extra point
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(true), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(0).NextDouble(0.5).NextDouble(0.5));

            // Test 4: Blocked kick
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false), new TestFluentSeedableRandom()
                .NextDouble(0.99).NextInt(1).NextDouble(0.5).NextDouble(0.5));

            // Test 5: Bad snap
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false), new TestFluentSeedableRandom()
                .NextDouble(0.01).NextDouble(0.5).NextDouble(0.5).NextDouble(0.5));
        }

        private void ExecuteFieldGoalScenario(Game game, TestFluentSeedableRandom rng)
        {
            var fieldGoal = new FieldGoal(rng);
            fieldGoal.Execute(game);
            // Just verify it doesn't throw
            Assert.IsNotNull(game.CurrentPlay);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithFieldGoalPlay(bool isExtraPoint)
        {
            var game = _testGame.GetGame();

            // Create a field goal play
            var fieldGoalPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                StartFieldPosition = 0,
                YardsGained = 0,
                IsExtraPoint = isExtraPoint,
                OffensePlayersOnField = new List<Player>(),
                DefensePlayersOnField = new List<Player>()
            };

            // Add kicker
            fieldGoalPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.K,
                LastName = "Kicker",
                Kicking = 70, // Good kicker
                Speed = 50,
                Strength = 50,
                Agility = 50
            });

            // Add holder (punter)
            fieldGoalPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.P,
                LastName = "Holder",
                Kicking = 50,
                Speed = 50,
                Strength = 50
            });

            // Add long snapper
            fieldGoalPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.LS,
                LastName = "Snapper",
                Blocking = 70,
                Speed = 50,
                Strength = 60
            });

            // Add defensive line for blocking attempts
            for (int i = 0; i < 3; i++)
            {
                fieldGoalPlay.DefensePlayersOnField.Add(new Player
                {
                    Position = Positions.DT,
                    LastName = $"Defender{i}",
                    Speed = 70,
                    Strength = 80,
                    Tackling = 70
                });
            }

            game.CurrentPlay = fieldGoalPlay;
            game.FieldPosition = 70; // Default 30-yard line
            game.YardsToGo = 10;
            game.CurrentDown = Downs.Fourth;
            game.HomeScore = 0;
            game.AwayScore = 0;

            return game;
        }

        #endregion
    }
}
