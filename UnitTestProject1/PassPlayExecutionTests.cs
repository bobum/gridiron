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