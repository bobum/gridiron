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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(8)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.5)        // Fail - tackled immediately
                .YACYards(2)
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(8)
                .PassCompletionCheck(0.9)        // Incomplete
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)        // FAIL - Sack!
                .SackYards(7)
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)        // FAIL - Sack!
                .SackYards(10)                   // Would be 10, limited by field position
                .ElapsedTimeRandomFactor(0.99);

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
            var rngNoPressure = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .QBPressureCheck(0.8)            // NO pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.59)       // 60% base - succeeds
                .YACOpportunityCheck(0.8)        // Fail
                .YACYards(3)
                .ElapsedTimeRandomFactor(0.99);

            // With pressure scenario (completion drops to ~40%)
            var rngPressure = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)
                .QBPressureCheck(0.2)            // PRESSURE!
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.59)       // 40% with pressure - fails
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.10)     // SCREEN (< 0.15)
                .AirYards(1)                     // Screen: -3 to +3 range
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // Success
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)        // PASS - Protection holds (lower = success)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.90)     // DEEP (> 0.85)
                .AirYards(30)                    // Deep: 18-44 range
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // Success
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // SUCCESS - breaks tackles
                .YACRandomFactor(0.9)            // Adds to base YAC
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);

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
            
            // Modify ALL WR receivers to have big play potential
            var receivers = game.CurrentPlay.OffensePlayersOnField.Where(p => p.Position == Positions.WR).ToList();
            foreach (var receiver in receivers)
            {
                receiver.Speed = 95; // Fast receiver for big play potential
                receiver.Agility = 92;
                receiver.Rushing = 88;
            }

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(15)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // SUCCESS - breaks tackles
                .YACRandomFactor(0.5)            // Moderate random factor
                .BigPlayCheck(0.04)              // BIG PLAY! (< 0.05)
                .BigPlayBonusYards(25)           // Extra yards from breaking free
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.9)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.9)      // Deep pass
                .AirYards(40)                    // Would be 40, limited by field
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // Success
                .YACRandomFactor(0.9)
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.9)          // High value = elite receiver
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // Success
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)        // Success
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)               // No big play
                .ElapsedTimeRandomFactor(0.99);               // 4 + 0.99*3 = 6.97 seconds (

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

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)        // FAIL - Sack!
                .SackYards(5)
                .ElapsedTimeRandomFactor(1.0);               // 2 + 1.0*2 = 4.0

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var timeAdded = game.CurrentPlay.ElapsedTime - initialElapsedTime;
            Assert.IsTrue(timeAdded >= 2.0 && timeAdded <= 4.0,
                $"Sack elapsed time ({timeAdded}) should be between 2 and 4 seconds");
        }

        #endregion

        #region SkillsCheckResult Integration Tests

        [TestMethod]
        public void PassPlay_SackYardsSkillsCheckResult_UsedForSacks()
        {
            // Arrange - Test that SackYardsSkillsCheckResult correctly calculates sack yardage
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 50;
            SetPlayerSkills(game, 40, 90);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.95)       // FAIL - Sack!
                .SackYards(8)                     // SackYardsSkillsCheckResult should use this
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - Should have -8 yards (from SackYardsSkillsCheckResult)
            Assert.AreEqual(-8, game.CurrentPlay.YardsGained,
                "SackYardsSkillsCheckResult should calculate sack yardage");
        }

        [TestMethod]
        public void PassPlay_SackYardsSkillsCheckResult_ClampsToFieldPosition()
        {
            // Arrange - Test that SackYardsSkillsCheckResult respects field position
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 4;  // Very close to own goal line
            SetPlayerSkills(game, 40, 90);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.95)       // FAIL - Sack!
                .SackYards(10)                    // Would be -10, but clamped to -4
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - Should be clamped to -4 (field position)
            Assert.AreEqual(-4, game.CurrentPlay.YardsGained,
                "SackYardsSkillsCheckResult should clamp to field position");
        }

        [TestMethod]
        public void PassPlay_AirYardsSkillsCheckResult_ScreenPass()
        {
            // Arrange - Test that AirYardsSkillsCheckResult calculates screen pass air yards
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            SetPlayerSkills(game, 70, 70);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.10)     // Screen pass (< 0.15)
                .AirYards(-2)                     // AirYardsSkillsCheckResult for screen: -3 to +2
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.9)        // Fail - tackled immediately
                .ImmediateTackleYards(1)
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(PassType.Screen, passPlay.PassSegments[0].Type);
            Assert.IsTrue(passPlay.PassSegments[0].AirYards >= -3 && passPlay.PassSegments[0].AirYards < 3,
                "AirYardsSkillsCheckResult should calculate screen pass air yards (-3 to +2)");
        }

        [TestMethod]
        public void PassPlay_AirYardsSkillsCheckResult_DeepPassNearGoalLine()
        {
            // Arrange - Test that AirYardsSkillsCheckResult clamps to field position
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 88; // 12 yards from goal line
            SetPlayerSkills(game, 70, 70);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.90)     // Deep pass (> 0.85)
                .AirYards(18)                     // Would be 18-44, but clamped to 12 (only 12 yards to goal)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.9)        // Fail
                .ImmediateTackleYards(1)
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].AirYards >= 12 && passPlay.PassSegments[0].AirYards < 13,
                "AirYardsSkillsCheckResult should clamp deep pass to remaining field (12 yards)");
        }

        [TestMethod]
        public void PassPlay_YardsAfterCatchSkillsCheckResult_ImmediateTackle()
        {
            // Arrange - Test YardsAfterCatchSkillsCheckResult when receiver tackled immediately
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.9)        // FAIL - tackled immediately
                .ImmediateTackleYards(2)         // YardsAfterCatchSkillsCheckResult returns 0-2
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch >= 0 &&
                         passPlay.PassSegments[0].YardsAfterCatch < 3,
                "YardsAfterCatchSkillsCheckResult should return 0-2 yards when tackled immediately");
        }

        [TestMethod]
        public void PassPlay_YardsAfterCatchSkillsCheckResult_GoodYAC()
        {
            // Arrange - Test YardsAfterCatchSkillsCheckResult with successful YAC opportunity
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            // Set receiver skills for predictable YAC calculation
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);
            receiver.Speed = 80;
            receiver.Agility = 75;
            receiver.Rushing = 70;  // Average = 75, baseYAC = 3 + 75/20 = 6.75

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.2)        // SUCCESS - breaks tackles
                .YACRandomFactor(0.5)            // Random factor: 0.5 * 8 - 2 = 2
                .BigPlayCheck(0.5)               // No big play
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            // baseYAC = 3 + 75/20 = 6.75, randomFactor = 2, total ≈ 9
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch >= 7 &&
                         passPlay.PassSegments[0].YardsAfterCatch <= 11,
                $"YardsAfterCatchSkillsCheckResult should calculate good YAC (got {passPlay.PassSegments[0].YardsAfterCatch})");
        }

        [TestMethod]
        public void PassPlay_YardsAfterCatchSkillsCheckResult_BigPlay()
        {
            // Arrange - Test YardsAfterCatchSkillsCheckResult with big play
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            // Set ALL WR receivers to be fast (speed > 85) for big play eligibility
            var receivers = game.CurrentPlay.OffensePlayersOnField.Where(p => p.Position == Positions.WR).ToList();
            foreach (var r in receivers)
            {
                r.Speed = 90;
                r.Agility = 88;
                r.Rushing = 80;  // Average = 86
            }

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.2)        // SUCCESS
                .YACRandomFactor(0.5)            // Random factor: 0.5 * 8 - 2 = 2
                .BigPlayCheck(0.03)              // BIG PLAY! (< 0.05 and speed > 85)
                .BigPlayBonusYards(20)           // Extra 20 yards
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch >= 25,
                $"YardsAfterCatchSkillsCheckResult should add big play bonus (got {passPlay.PassSegments[0].YardsAfterCatch})");
        }

        [TestMethod]
        public void PassPlay_YardsAfterCatchSkillsCheckResult_SlowReceiverNoBigPlay()
        {
            // Arrange - Test that slow receiver (speed <= 85) doesn't get big play
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            // Set all WR receivers to be slow (speed <= 85)
            var receivers = game.CurrentPlay.OffensePlayersOnField.Where(p => p.Position == Positions.WR).ToList();
            foreach (var r in receivers)
            {
                r.Speed = 80;   // Not fast enough for big play
                r.Agility = 75;
                r.Rushing = 70;
            }

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.2)        // SUCCESS
                .YACRandomFactor(0.5)            // Random factor: 2
                .BigPlayCheck(0.03)              // Would trigger, but speed 80 < 85
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch < 20,
                "YardsAfterCatchSkillsCheckResult should NOT trigger big play for slow receiver");
        }

        [TestMethod]
        public void PassPlay_AllSkillsCheckResults_IntegrationTest()
        {
            // Arrange - Complete integration test using all three SkillsCheckResults
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 25;
            SetPlayerSkills(game, 75, 70);

            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);
            receiver.Speed = 90;
            receiver.Agility = 88;
            receiver.Rushing = 85;

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)        // Protection holds
                .QBPressureCheck(0.5)            // No pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(15)                    // AirYardsSkillsCheckResult: 15 yards
                .PassCompletionCheck(0.5)        // Complete
                .YACOpportunityCheck(0.2)        // YAC success
                .YACRandomFactor(0.75)           // Good random factor
                .BigPlayCheck(0.5)               // No big play
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - Verify all components work together
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Pass should be complete");
            Assert.AreEqual(15, passPlay.PassSegments[0].AirYards,
                "AirYardsSkillsCheckResult should set air yards to 15");
            Assert.IsTrue(passPlay.PassSegments[0].YardsAfterCatch > 5,
                "YardsAfterCatchSkillsCheckResult should calculate YAC");
            Assert.AreEqual(passPlay.PassSegments[0].AirYards + passPlay.PassSegments[0].YardsAfterCatch,
                passPlay.YardsGained,
                "Total yards should equal air yards + YAC");
        }

        [TestMethod]
        public void PassPlay_AllSkillsCheckResults_SackScenario()
        {
            // Arrange - Integration test for sack scenario using SackYardsSkillsCheckResult
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 35;
            SetPlayerSkills(game, 40, 90);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.95)       // FAIL - Sack!
                .SackYards(6)                     // SackYardsSkillsCheckResult: -6 yards
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - Verify sack components work together
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Sack should be incomplete");
            Assert.AreEqual(-6, passPlay.YardsGained,
                "SackYardsSkillsCheckResult should set yards to -6");
            Assert.AreEqual(0, passPlay.PassSegments[0].AirYards,
                "Sack should have 0 air yards");
            Assert.AreEqual(0, passPlay.PassSegments[0].YardsAfterCatch,
                "Sack should have 0 YAC");
        }

        #endregion

        #region Comprehensive Skill x Protection x Coverage Matrix Tests

        [TestMethod]
        public void Matrix_HighSkills_GoodProtection_WeakCoverage()
        {
            // Arrange - Elite QB/receivers vs weak defense, good protection
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 90, 50); // Strong offense vs weak defense

            var rng = CreateRngForCompletedPass(airYards: 15, yacYards: 8, protectionSucceeds: true);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "High skills with good protection should complete");
            Assert.IsTrue(passPlay.YardsGained >= 20,
                $"Elite offense should gain significant yards (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_HighSkills_GoodProtection_StrongCoverage()
        {
            // Arrange - Elite QB/receivers vs strong coverage, good protection
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 90, 85); // Strong offense vs strong defense

            var rng = CreateRngForCompletedPass(airYards: 12, yacYards: 3, protectionSucceeds: true);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "High skills should overcome strong coverage");
            Assert.IsTrue(passPlay.YardsGained >= 10,
                $"Should still gain decent yards despite coverage (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_HighSkills_BadProtection_WeakCoverage()
        {
            // Arrange - Elite QB but bad protection
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 90, 50);

            var rng = CreateRngForCompletedPass(airYards: 10, yacYards: 5, protectionSucceeds: false);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert - QB under pressure but weak coverage allows completion
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Should complete despite pressure");
        }

        [TestMethod]
        public void Matrix_EvenSkills_GoodProtection_EvenCoverage()
        {
            // Arrange - Average matchup across the board
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 70);

            var rng = CreateRngForCompletedPass(airYards: 10, yacYards: 4, protectionSucceeds: true);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Even matchup should allow completions");
            Assert.IsTrue(passPlay.YardsGained >= 10 && passPlay.YardsGained <= 20,
                $"Even skills should produce average gains (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_EvenSkills_BadProtection_StrongCoverage()
        {
            // Arrange - Average skills but everything goes wrong
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 70, 85);

            var rng = CreateRngForIncompletePass(protectionSucceeds: false);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete,
                "Bad protection and strong coverage should cause incompletion");
        }

        [TestMethod]
        public void Matrix_LowSkills_GoodProtection_WeakCoverage()
        {
            // Arrange - Weak QB/receivers but favorable conditions
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 50, 40);

            var rng = CreateRngForCompletedPass(airYards: 8, yacYards: 2, protectionSucceeds: true);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete,
                "Favorable conditions should allow weak offense to complete");
            Assert.IsTrue(passPlay.YardsGained <= 15,
                $"Low skills should limit yardage (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_LowSkills_BadProtection_StrongCoverage()
        {
            // Arrange - Worst case scenario: weak offense, bad protection, strong coverage
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 40, 90);

            var rng = CreateRngForIncompletePass(protectionSucceeds: false);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete,
                "Weak offense with bad protection should struggle");
        }

        [TestMethod]
        public void Matrix_Sack_WeakProtection_StrongRush()
        {
            // Arrange - Classic sack scenario: weak O-line vs strong pass rush
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 40, 90);

            var rng = CreateRngForSack(sackYards: 7);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Sack should be incomplete");
            Assert.IsTrue(passPlay.YardsGained < 0,
                $"Sack should result in negative yards (got {passPlay.YardsGained})");
            Assert.IsTrue(passPlay.YardsGained <= -5,
                $"Strong rush should produce significant loss (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_Sack_WeakProtection_EvenRush()
        {
            // Arrange - Weak protection even against average rush
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 35, 70);

            var rng = CreateRngForSack(sackYards: 5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.IsFalse(passPlay.PassSegments[0].IsComplete, "Should be sacked");
            Assert.AreEqual(-5, passPlay.YardsGained, "Should lose 5 yards");
        }

        [TestMethod]
        public void Matrix_ScreenPass_QuickRelease_BeatsBlitz()
        {
            // Arrange - Screen pass with quick release beats aggressive defense
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 75, 80);

            var rng = CreateRngForScreenPass(airYards: -1, yacYards: 8);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(PassType.Screen, passPlay.PassSegments[0].Type, "Should be screen pass");
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Screen should be complete");
            Assert.IsTrue(passPlay.YardsGained >= 5,
                $"Screen with good YAC should gain yards (got {passPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_DeepPass_HighSkills_BigPlay()
        {
            // Arrange - Deep pass with elite QB and fast receiver
            var game = CreateGameWithPassPlay();
            SetPlayerSkills(game, 90, 65);

            // Set receivers to be fast for deep threat
            var receivers = game.CurrentPlay.OffensePlayersOnField.Where(p => p.Position == Positions.WR).ToList();
            foreach (var r in receivers)
            {
                r.Speed = 95;
                r.Agility = 90;
            }

            var rng = CreateRngForDeepPass(airYards: 35, yacYards: 15);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);

            // Assert
            var passPlay = (PassPlay)game.CurrentPlay;
            Assert.AreEqual(PassType.Deep, passPlay.PassSegments[0].Type, "Should be deep pass");
            Assert.IsTrue(passPlay.PassSegments[0].IsComplete, "Deep pass should connect");
            Assert.IsTrue(passPlay.YardsGained >= 40,
                $"Deep completion should gain major yards (got {passPlay.YardsGained})");
        }

        #endregion

        #region Helper Methods

        private TestFluentSeedableRandom CreateRngForCompletedPass(int airYards, int yacYards, bool protectionSucceeds)
        {
            double protectionCheck = protectionSucceeds ? 0.3 : 0.7; // < 0.5 succeeds

            return new TestFluentSeedableRandom()
                .PassProtectionCheck(protectionCheck)
                .QBPressureCheck(0.5)           // No additional pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)     // Forward pass
                .AirYards(airYards)
                .PassCompletionCheck(0.4)       // Complete (< 0.5 + skill bonus)
                .ImmediateTackleYards(2)        // For immediate tackle scenario (if YAC fails)
                .YACOpportunityCheck(0.3)       // YAC opportunity succeeds
                .YACRandomFactor(0.5)
                .BigPlayCheck(0.9)              // No big play
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForIncompletePass(bool protectionSucceeds)
        {
            double protectionCheck = protectionSucceeds ? 0.3 : 0.7;

            return new TestFluentSeedableRandom()
                .PassProtectionCheck(protectionCheck)
                .QBPressureCheck(0.8)           // Heavy pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)     // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.9)       // Incomplete (high value = fail)
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForSack(int sackYards)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.95)      // FAIL - Sack!
                .SackYards(sackYards)
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForScreenPass(int airYards, int yacYards)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.10)    // Screen (< 0.15)
                .AirYards(airYards)
                .PassCompletionCheck(0.3)       // Complete
                .YACOpportunityCheck(0.3)       // YAC success
                .YACRandomFactor(0.7)
                .BigPlayCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
        }

        private TestFluentSeedableRandom CreateRngForDeepPass(int airYards, int yacYards)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.2)       // Excellent protection
                .QBPressureCheck(0.3)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.90)    // Deep (> 0.85)
                .AirYards(airYards)
                .PassCompletionCheck(0.3)       // Complete
                .YACOpportunityCheck(0.2)       // YAC success
                .YACRandomFactor(0.8)
                .BigPlayCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
        }

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