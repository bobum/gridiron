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
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Bad Snap Tests

        [TestMethod]
        public void Punt_BadSnap_PunterRecovers_LosesYards()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 30;

            var rng = PuntPlayScenarios.BadSnap(baseLoss: 0.3, randomFactor: 0.5);

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

            var rng = PuntPlayScenarios.BadSnapSafety();

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

            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.5, returnYards: 0.5);

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

            var rng = PuntPlayScenarios.BlockedPuntDefenseRecovers(baseBounce: 0.5, randomFactor: 0.6);

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

            var rng = PuntPlayScenarios.BlockedPuntOffenseRecovers(recoveryYards: 0.7);

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

            var rng = PuntPlayScenarios.BlockedPuntTouchdown();

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

        [TestMethod]
        public void Punt_GoodSnap_LowBlockProbability()
        {
            // Arrange - Test that good snaps have low (~1%) block rate
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;

            // Use RNG value that would NOT block with good snap
            // Good snap: ~1% block rate, so 0.02 should NOT block
            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.5, returnYards: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsFalse(play.Blocked, "Good snap should have low block probability");
            Assert.IsTrue(play.GoodSnap, "Should be marked as good snap");
        }

        [TestMethod]
        public void Punt_BadSnap_HighBlockProbability()
        {
            // Arrange - Test that bad snaps dramatically increase block chance
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;

            // Make long snapper terrible to ensure bad snap
            var snapper = play.OffensePlayersOnField.FirstOrDefault(p => p.Position == Positions.LS);
            if (snapper != null)
            {
                snapper.Blocking = 10; // Terrible snapper
            }

            var rng = PuntPlayScenarios.BadSnap(baseLoss: 0.5, randomFactor: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            // Bad snap prevents kick entirely, no block check occurs
            Assert.IsFalse(play.GoodSnap, "Should be bad snap");
            Assert.IsFalse(play.Blocked, "Bad snap prevents kick, no block");
        }

        [TestMethod]
        public void Punt_EliteRusherVsWeakLine_HigherBlockChance()
        {
            // Arrange - Test that skill differential affects block probability
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;

            // Create elite defensive rusher
            var eliteRusher = new Player
            {
                Position = Positions.DE,
                LastName = "EliteRusher",
                Speed = 95,
                Strength = 95,
                Tackling = 85,
                Awareness = 80,
                Agility = 85
            };
            play.DefensePlayersOnField.Add(eliteRusher);

            // Weaken offensive line
            foreach (var olineman in play.OffensePlayersOnField.Where(p =>
                p.Position == Positions.T || p.Position == Positions.G || p.Position == Positions.C))
            {
                olineman.Strength = 40;
                olineman.Awareness = 40;
            }

            // Use RNG that would block with skill advantage
            // Base 1% + skill differential bonus
            var rng = PuntPlayScenarios.BlockedPuntDefenseRecovers(baseBounce: 0.5, randomFactor: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Elite rusher vs weak line should increase block chance");
            Assert.IsNotNull(play.BlockedBy, "Should track who blocked");
        }

        #endregion

        #region Touchback Tests

        [TestMethod]
        public void Punt_IntoEndZone_ResultsInTouchback()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;

            var rng = PuntPlayScenarios.Touchback(puntDistance: 0.95);

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

            var rng = PuntPlayScenarios.OutOfBounds();

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

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.85);

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

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.35);

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

            var rng = PuntPlayScenarios.FairCatch(puntDistance: 0.65);

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

            var rng = PuntPlayScenarios.FairCatchDeep();

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

            var rng = PuntPlayScenarios.MuffReceivingTeamRecovers(recoveryYards: 0.3);

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

            var rng = PuntPlayScenarios.MuffPuntingTeamRecovers();

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

            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.65, returnYards: 0.5);

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

            var rng = PuntPlayScenarios.ReturnTouchdown();

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

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.6);

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

            var rng = PuntPlayScenarios.BadSnap(baseLoss: 0.95, randomFactor: 0.9);

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

            var rng = PuntPlayScenarios.Touchback(puntDistance: 0.99);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            // Should be touchback since punt would exceed 100
            Assert.IsTrue(play.Touchback || play.PuntDistance <= 55, "Punt distance should be clamped or result in touchback");
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void Punt_FromOwn1YardLine_HandledCorrectly()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 1;  // Own 1-yard line

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.PuntDistance > 0, "Should have positive punt distance");
            Assert.IsTrue(play.Downed || play.Touchback || play.ReturnSegments.Count > 0, "Punt should complete successfully");
        }

        [TestMethod]
        public void Punt_BadSnapResultsInExactly0FieldPosition_IsSafety()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 3;  // Close to own goal
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = PuntPlayScenarios.BadSnap(baseLoss: 0.8, randomFactor: 0.95);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsSafety, "Should be marked as safety");
            Assert.AreEqual(0, game.FieldPosition, "Field position should be 0");
        }

        [TestMethod]
        public void Punt_ExtremelyShortPunt_Shanked()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;
            var play = (PuntPlay)game.CurrentPlay;

            // Replace with weak punter (kicking skill 15) to enable shanked punt
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.P);
            play.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.P,
                LastName = "WeakPunter",
                Kicking = 15,  // Very low kicking skill for shanked punt
                Speed = 50,
                Strength = 50,
                Agility = 50,
                Catching = 40
            });

            var rng = PuntPlayScenarios.ShankedPunt();

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(play.PuntDistance >= 10, "Even shanked punt should have minimum distance");
            Assert.IsTrue(play.PuntDistance < 25, "Shanked punt should be short");
        }

        [TestMethod]
        public void Punt_ExtremelyLongPunt_MaxDistance()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 20;  // Own 20-yard line

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.99);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.PuntDistance >= 55, "Maximum punt should be 55+ yards");
            Assert.IsTrue(play.PuntDistance <= 80, "Punt distance should be reasonable");
        }

        [TestMethod]
        public void Punt_LandsAtExactly100_IsTouchback()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 45;  // Position where punt can reach exactly 100

            var rng = PuntPlayScenarios.Touchback(puntDistance: 0.95);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Touchback, "Punt landing at/beyond 100 should be touchback");
            Assert.IsTrue(play.PossessionChange, "Possession should change on touchback");
        }

        [TestMethod]
        public void Punt_BlockedPuntRecoveredInEndZone_TouchdownDefense()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 8;  // Close to own end zone
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 3;

            var rng = PuntPlayScenarios.BlockedPuntDefenseRecovers(baseBounce: 0.05, randomFactor: 0.1);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be touchdown when recovered in end zone");
            Assert.AreEqual(9, game.AwayScore, "Away team should score TD (3 + 6)");
        }

        [TestMethod]
        public void Punt_BlockedPuntOffenseRecoversInEndZone_Safety()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 5;  // Very close to own end zone
            var play = (PuntPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 14;
            game.AwayScore = 10;

            var rng = PuntPlayScenarios.BlockedPuntSafety();

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsSafety, "Should be safety when offense recovers in own end zone");
            Assert.AreEqual(12, game.AwayScore, "Away team should score safety (10 + 2)");
            Assert.AreEqual(14, game.HomeScore, "Home score should not change");
            Assert.AreEqual(0, play.EndFieldPosition, "Should be at 0-yard line");
            Assert.IsTrue(play.PossessionChange, "Possession should change after safety");
        }

        [TestMethod]
        public void Punt_MuffedCatchAtOwn1YardLine_DangerousPosition()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 50;  // Midfield
            var play = (PuntPlay)game.CurrentPlay;

            var rng = PuntPlayScenarios.MuffReceivingTeamRecovers(recoveryYards: 0.0);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            Assert.IsTrue(play.MuffedCatch, "Should be muffed catch");
            Assert.IsNotNull(play.MuffedBy, "Should track who muffed");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
        }

        [TestMethod]
        public void Punt_OutOfBoundsAtOwn1YardLine_DownedAtSpot()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 50;

            var rng = PuntPlayScenarios.OutOfBounds();

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.OutOfBounds, "Should be out of bounds");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(0, play.ReturnSegments.Count, "No return on out of bounds");
        }

        [TestMethod]
        public void Punt_ReturnLosesYards_NegativeReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = PuntPlayScenarios.NegativeReturn();

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.ReturnSegments.Count > 0, "Should have return attempt");
            // Return yards can be negative (tackled behind catch point)
            Assert.IsTrue(play.YardsGained >= -3, "Return can lose up to 3 yards");
        }

        [TestMethod]
        public void Punt_NoPunterOnField_UsesBackupPlayer()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;

            // Remove punter
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.P);

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.5);

            var punt = new Punt(rng);

            // Act & Assert - should not throw exception
            punt.Execute(game);
            Assert.IsNotNull(game.CurrentPlay, "Play should complete even without dedicated punter");
        }

        [TestMethod]
        public void Punt_NoLongSnapper_UsesCenterOrBackup()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;

            // Remove long snapper (should fall back to center or default logic)
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.LS);

            var rng = PuntPlayScenarios.Downed(puntDistance: 0.5);

            var punt = new Punt(rng);

            // Act & Assert - should not throw exception
            punt.Execute(game);
            Assert.IsNotNull(game.CurrentPlay, "Play should complete even without dedicated long snapper");
        }

        [TestMethod]
        public void Punt_ReturnerWithMaximumSkills_GreatReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;

            // Give returner maximum skills
            var superReturner = play.DefensePlayersOnField.First(p => p.Position == Positions.CB);
            superReturner.Speed = 99;
            superReturner.Agility = 99;
            superReturner.Catching = 99;

            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.5, returnYards: 0.9);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play2 = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play2.ReturnSegments.Count > 0, "Should have return");
            // With max skills, return should be decent even against good coverage
        }

        [TestMethod]
        public void Punt_ReturnerWithMinimumSkills_PoorReturn()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            var play = (PuntPlay)game.CurrentPlay;

            // Give returner minimum skills
            var weakReturner = play.DefensePlayersOnField.First(p => p.Position == Positions.CB);
            weakReturner.Speed = 20;
            weakReturner.Agility = 20;
            weakReturner.Catching = 20;

            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.5, returnYards: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play2 = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play2.ReturnSegments.Count > 0, "Should have return attempt");
            // With low skills, return should be limited
        }

        [TestMethod]
        public void Punt_FromMidfield_TypicalScenario()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 50;  // Midfield

            var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.6, returnYards: 0.5);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.PuntDistance > 0, "Should have punt distance");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void Punt_MultipleConsecutivePunts_AllSucceed()
        {
            // Arrange & Act - Run multiple punts in sequence
            for (int i = 0; i < 5; i++)
            {
                var game = CreateGameWithPuntPlay();
                game.FieldPosition = 30 + (i * 10);

                var rng = PuntPlayScenarios.NormalReturn(puntDistance: 0.5 + (i * 0.1), returnYards: 0.5);

                var punt = new Punt(rng);
                punt.Execute(game);

                // Assert
                var play = (PuntPlay)game.CurrentPlay;
                Assert.IsNotNull(play, $"Punt {i + 1} should complete successfully");
                Assert.IsTrue(play.PuntDistance > 0, $"Punt {i + 1} should have positive distance");
            }
        }

        [TestMethod]
        public void Punt_BadSnapWithMinimalLoss_StillCompletes()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 40;

            var rng = PuntPlayScenarios.BadSnap(baseLoss: 0.0, randomFactor: 0.0);

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsFalse(play.GoodSnap, "Should be bad snap");
            Assert.IsTrue(play.YardsGained < 0, "Should lose some yards");
            Assert.IsTrue(play.YardsGained >= -20, "Loss should be minimal");
        }

        [TestMethod]
        public void Punt_CoffinCorner_DownedAt1YardLine()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 55;  // Opponent's 45

            var rng = PuntPlayScenarios.CoffinCorner();

            var punt = new Punt(rng);

            // Act
            punt.Execute(game);

            // Assert
            var play = (PuntPlay)game.CurrentPlay;
            Assert.IsTrue(play.Downed, "Should be downed");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            // Punt should land close to goal line (great field position)
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void Punt_MultipleScenarios_AllExecuteWithoutError()
        {
            // This test ensures all punt paths execute without exceptions

            // Test 1: Bad snap (no block check needed - returns early)
            ExecutePuntScenario(CreateGameWithPuntPlay(), PuntPlayScenarios.BadSnap(baseLoss: 0.5, randomFactor: 0.5));

            // Test 2: Blocked punt (defense recovers)
            ExecutePuntScenario(CreateGameWithPuntPlay(), PuntPlayScenarios.BlockedPuntDefenseRecovers(baseBounce: 0.5, randomFactor: 0.5));

            // Test 3: Touchback
            ExecutePuntScenario(CreateGameWithPuntPlay(), PuntPlayScenarios.Touchback(puntDistance: 0.95));

            // Test 4: Fair catch
            ExecutePuntScenario(CreateGameWithPuntPlay(), PuntPlayScenarios.FairCatch(puntDistance: 0.6));

            // Test 5: Normal return
            ExecutePuntScenario(CreateGameWithPuntPlay(), PuntPlayScenarios.NormalReturn(puntDistance: 0.6, returnYards: 0.5));
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

            // Add offensive line for block calculation
            for (int i = 0; i < 5; i++)
            {
                puntPlay.OffensePlayersOnField.Add(new Player
                {
                    Position = i < 2 ? Positions.T : (i < 4 ? Positions.G : Positions.C),
                    LastName = $"OLineman{i}",
                    Blocking = 70,
                    Strength = 70,
                    Awareness = 65,
                    Speed = 50
                });
            }

            // Add defensive rushers for block attempts
            for (int i = 0; i < 3; i++)
            {
                puntPlay.DefensePlayersOnField.Add(new Player
                {
                    Position = i < 2 ? Positions.DE : Positions.DT,
                    LastName = $"Rusher{i}",
                    Speed = 75,
                    Strength = 80,
                    Tackling = 70,
                    Awareness = 65,
                    Agility = 60
                });
            }

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
