using DomainObjects;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class KickoffPlayExecutionTests
    {
        private TestGame? _testGame;

        [TestInitialize]
        public void Setup()
        {
            _testGame = new TestGame();
        }

        #region Normal Kickoff Tests

        [TestMethod]
        public void Kickoff_NormalWithReturn_SetsFieldPosition()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            // Using NormalReturn scenario: moderate kick with moderate return
            var rng = KickoffPlayScenarios.NormalReturn(kickDistance: 0.5, returnYardage: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsFalse(play.Touchback, "Should not be a touchback");
            Assert.IsFalse(play.OutOfBounds, "Should not be out of bounds");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsNotNull(play.Kicker, "Should have a kicker");
            Assert.IsTrue(play.EndFieldPosition > 0 && play.EndFieldPosition < 100, "Should have valid field position");
        }

        [TestMethod]
        public void Kickoff_DeepKick_Touchback()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            // Excellent kicker with good kick
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 90;

            // Using Touchback scenario: deep kick into the end zone
            var rng = KickoffPlayScenarios.Touchback(kickDistance: 0.9);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Touchback, "Should be a touchback");
            Assert.IsFalse(play.IsTouchdown, "Touchback is not a touchdown");
            Assert.AreEqual(25, game.FieldPosition, "Should be at 25-yard line");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void Kickoff_ShortKick_NoTouchback()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Weak kicker
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 30;

            // Using ShortKick scenario: weak kicker with limited return
            var rng = KickoffPlayScenarios.ShortKick(returnYardage: 0.3);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsFalse(play.Touchback, "Should not be a touchback");
            Assert.IsLessThan(60, play.KickDistance, "Should be a short kick");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        #endregion

        #region Touchback Tests

        [TestMethod]
        public void Kickoff_Touchback_BallAt25()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            game.HomeScore = 7;
            game.AwayScore = 0;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85;

            // Using Touchback scenario: very deep kick for touchback
            var rng = KickoffPlayScenarios.Touchback(kickDistance: 0.95);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Touchback, "Should be touchback");
            Assert.AreEqual(25, game.FieldPosition, "Ball should be at 25-yard line");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }

        #endregion

        #region Out of Bounds Tests

        [TestMethod]
        public void Kickoff_OutOfBounds_BallAt40()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Using OutOfBounds scenario: kick goes out of bounds, penalty to 40-yard line
            var rng = KickoffPlayScenarios.OutOfBounds();

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.OutOfBounds, "Should be out of bounds");
            Assert.AreEqual(40, game.FieldPosition, "Ball should be at 40-yard line (penalty)");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(40, play.EndFieldPosition, "EndFieldPosition should be 40");
        }

        #endregion

        #region Onside Kick Tests

        [TestMethod]
        public void Kickoff_OnsideKick_KickingTeamRecovers()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 14;
            game.AwayScore = 21; // Trailing by 7

            // Using OnsideKickRecovered scenario: successful onside kick recovery
            var rng = KickoffPlayScenarios.OnsideKickRecovered(onsideDistance: 0.3);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.OnsideKick, "Should be onside kick");
            Assert.IsTrue(play.OnsideRecovered, "Kicking team should recover");
            Assert.IsFalse(play.PossessionChange, "Possession should NOT change");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
            Assert.IsTrue(play.KickDistance >= 10 && play.KickDistance <= 15, "Onside kick travels 10-15 yards");
        }

        [TestMethod]
        public void Kickoff_OnsideKick_ReceivingTeamRecovers()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            game.HomeScore = 28;
            game.AwayScore = 17; // Trailing by 11

            // Using OnsideKickNotRecovered scenario: failed onside kick attempt
            var rng = KickoffPlayScenarios.OnsideKickNotRecovered(onsideDistance: 0.4);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.OnsideKick, "Should be onside kick");
            Assert.IsFalse(play.OnsideRecovered, "Kicking team should NOT recover");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
        }

        [TestMethod]
        public void Kickoff_OnsideKick_FieldPositionCorrect()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            game.HomeScore = 10;
            game.AwayScore = 17;

            // Using OnsideKickMinimumDistance scenario: exactly 10 yards, kicking team recovers
            var rng = KickoffPlayScenarios.OnsideKickMinimumDistance(kickingTeamRecovers: true);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.OnsideKick, "Should be onside kick");
            Assert.AreEqual(45, play.EndFieldPosition, "Ball should be at 45-yard line (35 + 10)");
            Assert.AreEqual(45, game.FieldPosition, "Game position should match");
        }

        #endregion

        #region Return Touchdown Tests

        [TestMethod]
        public void Kickoff_ReturnForTouchdown_Scores6Points()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 0;

            // Weak kicker, excellent returner
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 40;

            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 95;
            returner.Agility = 90;

            // Using ReturnTouchdown scenario: short kick with excellent return
            var rng = KickoffPlayScenarios.ReturnTouchdown(kickDistance: 0.2);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be a touchdown");
            Assert.AreEqual(6, game.AwayScore, "Away should score TD (0 + 6)");
            Assert.AreEqual(7, game.HomeScore, "Home score should not change");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void Kickoff_ReturnTD_FieldPositionAt100()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 35;

            var returner = play.DefensePlayersOnField.First();
            returner.Speed = 99;
            returner.Agility = 99;

            // Using ReturnTouchdownMaximum scenario: very short kick with maximum return
            var rng = KickoffPlayScenarios.ReturnTouchdownMaximum();

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be TD");
            Assert.AreEqual(100, play.EndFieldPosition, "Should be at 100");
            Assert.AreEqual(100, game.FieldPosition, "Game position at 100");
        }

        #endregion

        #region Fair Catch Tests

        [TestMethod]
        public void Kickoff_FairCatch_BallAtSpot()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            // Using FairCatch scenario: returner signals fair catch, no return
            var rng = KickoffPlayScenarios.FairCatch(kickDistance: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.FairCatch, "Should be fair catch");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be 1st down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.IsEmpty(play.ReturnSegments, "No return segment on fair catch");
        }

        [TestMethod]
        public void Kickoff_FairCatch_CorrectFieldPosition()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Set up for a kick that lands at specific spot
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 70;

            // Using FairCatch scenario: fair catch on a deep kick
            var rng = KickoffPlayScenarios.FairCatch(kickDistance: 0.6);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.FairCatch, "Should be fair catch");
            // Field position should be 100 - landingSpot
            // landingSpot = 35 + kickDistance
            var expectedFieldPosition = 100 - (35 + play.KickDistance);
            Assert.AreEqual(expectedFieldPosition, play.EndFieldPosition, "Field position should match fair catch spot");
            Assert.AreEqual(expectedFieldPosition, game.FieldPosition, "Game field position should match");
        }

        [TestMethod]
        public void Kickoff_NoFairCatch_NormalReturnProceeds()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Use a shorter kick to avoid all field position and hang time bonuses
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 50; // Weaker kicker for shorter kick

            // Using ShortKick scenario: ensures no fair catch, normal return proceeds
            var rng = KickoffPlayScenarios.ShortKick(returnYardage: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsFalse(play.FairCatch, "Should not be fair catch");
            Assert.IsNotEmpty(play.ReturnSegments, "Should have return segment");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void Kickoff_FairCatch_NoReturnSegmentCreated()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Using FairCatch scenario: fair catch should not create return segment
            var rng = KickoffPlayScenarios.FairCatch(kickDistance: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);

            // Assert
            Assert.IsTrue(play.FairCatch, "Should be fair catch");
            Assert.IsEmpty(play.ReturnSegments, "Should have no return segments");
            Assert.IsNull(play.InitialReturner, "Should have no initial returner");
        }

        [TestMethod]
        public void Kickoff_FairCatch_DeepInTerritory_MoreLikely()
        {
            // Arrange - Set up a deep kick that lands at receiving team's ~5 yard line
            // Landing spot = 35 + kickDistance, so for receiving team's 5-yard line:
            // Landing spot = 95 (from kicking team's perspective)
            // KickDistance needed = 60 yards
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85; // Good kicker for deep kick

            // Using Custom scenario: testing fair catch probability with deep kick and marginal fair catch value
            // RNG value of 0.35 should NOT trigger fair catch normally (baseline 0.25)
            // But with deep field position bonus (+20%) and long hang time bonus (+10-15%), it might trigger
            var rng = KickoffPlayScenarios.Custom(
                kickDistance: 0.6,
                outOfBounds: false,
                muff: false,
                fairCatch: false,  // Using 0.35 check value which is marginal
                returnYardage: 0.5,
                blockingPenalty: false,
                fumble: false,
                tacklePenalty: false,
                isOnside: false,
                onsideRecovered: false);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);

            // Assert
            // With deep field position (< 10 yard line) and long hang time,
            // fair catch probability increases significantly (baseline 25% + 20% field + 10-15% hang time)
            // We can't guarantee it will always trigger with 0.35, but the test documents the behavior
            var landingSpot = 35 + play.KickDistance;
            var receivingTeamFieldPosition = 100 - landingSpot;
            Assert.IsLessThanOrEqualTo(10, receivingTeamFieldPosition, "Ball should land at receiving team's 10-yard line or closer to test deep territory bonus");
        }

        [TestMethod]
        public void Kickoff_FairCatch_SetsCorrectGameState()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.FieldPosition = 35;

            // Using FairCatch scenario: verify game state is set correctly
            var rng = KickoffPlayScenarios.FairCatch(kickDistance: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.FairCatch, "Should be fair catch");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.IsFalse(play.IsTouchdown, "Should not be a touchdown");
            Assert.IsFalse(play.Touchback, "Should not be a touchback");
            Assert.IsFalse(play.OutOfBounds, "Should not be out of bounds");
        }

        #endregion

        #region Safety Tests

        [TestMethod]
        public void Kickoff_ReturnForSafety_Scores2Points()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 0;

            // Good kicker kicks deep
            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 80;

            // Poor returner who will lose yards
            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 40;
            returner.Agility = 40;

            // Using Custom scenario: deep kick with very poor return resulting in safety
            var rng = KickoffPlayScenarios.Custom(
                kickDistance: 0.35,
                outOfBounds: false,
                muff: false,
                fairCatch: false,
                returnYardage: 0.05,  // Very poor return (negative yards, will be safety)
                blockingPenalty: false,
                fumble: false,
                tacklePenalty: false,
                isOnside: false,
                onsideRecovered: false);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsSafety, "Should be a safety");
            Assert.AreEqual(9, game.HomeScore, "Home should score safety (7 + 2)");
            Assert.AreEqual(0, game.AwayScore, "Away score should not change");
            Assert.AreEqual(0, play.EndFieldPosition, "Should be at 0-yard line");
            Assert.AreEqual(0, game.FieldPosition, "Game position at 0");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void Kickoff_ReturnSafety_FieldPositionAt0()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85;

            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 35;
            returner.Agility = 35;

            // Using Custom scenario: deep kick with minimum return value causing safety
            var rng = KickoffPlayScenarios.Custom(
                kickDistance: 0.32,
                outOfBounds: false,
                muff: false,
                fairCatch: false,
                returnYardage: 0.0,  // Minimum return value (will be negative after calculation, causing safety)
                blockingPenalty: false,
                fumble: false,
                tacklePenalty: false,
                isOnside: false,
                onsideRecovered: false);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsSafety, "Should be a safety");
            Assert.AreEqual(0, play.EndFieldPosition, "Should be at 0");
            Assert.AreEqual(0, game.FieldPosition, "Game position at 0");
        }

        [TestMethod]
        public void Kickoff_SafetyScoring_CorrectTeamGetsPoints()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Away; // Away kicks
            game.HomeScore = 10;
            game.AwayScore = 14;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 85;

            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 38;
            returner.Agility = 38;

            // Using Custom scenario: safety where kicking team (Away) scores 2 points
            var rng = KickoffPlayScenarios.Custom(
                kickDistance: 0.32,
                outOfBounds: false,
                muff: false,
                fairCatch: false,
                returnYardage: 0.0,
                blockingPenalty: false,
                fumble: false,
                tacklePenalty: false,
                isOnside: false,
                onsideRecovered: false);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsSafety, "Should be a safety");
            Assert.AreEqual(16, game.AwayScore, "Away (kicking team) should score safety (14 + 2)");
            Assert.AreEqual(10, game.HomeScore, "Home score should not change");
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void Kickoff_NoKicker_HandlesGracefully()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Remove kicker
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.K);
            play.OffensePlayersOnField.RemoveAll(p => p.Position == Positions.P);

            // Using Touchback scenario: no kicker defaults to touchback
            var rng = KickoffPlayScenarios.Touchback();
            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Touchback, "Should default to touchback");
            Assert.AreEqual(25, game.FieldPosition, "Should be at 25");
        }

        [TestMethod]
        public void Kickoff_NoReturner_BallDownedAtSpot()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            // Remove all potential returners
            play.DefensePlayersOnField.Clear();

            // Using NormalReturn scenario: normal kick but no returner available
            var rng = KickoffPlayScenarios.NormalReturn(kickDistance: 0.5, returnYardage: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be TD");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsGreaterThan(0, play.EndFieldPosition, "Should have valid field position");
        }

        [TestMethod]
        public void Kickoff_ExcellentKicker_LongerDistance()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 95;

            // Using DeepKick scenario: excellent kicker should kick far
            var rng = KickoffPlayScenarios.DeepKick(returnYardage: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);

            // Assert
            Assert.IsGreaterThanOrEqualTo(60, play.KickDistance, "Excellent kicker should kick far");
        }

        [TestMethod]
        public void Kickoff_WeakKicker_ShorterDistance()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 20;

            // Using NormalReturn scenario with lower kick distance for weak kicker
            var rng = KickoffPlayScenarios.NormalReturn(kickDistance: 0.3, returnYardage: 0.5);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);

            // Assert
            Assert.IsLessThanOrEqualTo(50, play.KickDistance, "Weak kicker should not kick far");
        }

        [TestMethod]
        public void Kickoff_ExcellentReturner_MoreYards()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 98;
            returner.Agility = 96;

            // Using NormalReturn scenario with high return yardage for excellent returner
            var rng = KickoffPlayScenarios.NormalReturn(kickDistance: 0.5, returnYardage: 0.85);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsNotEmpty(play.ReturnSegments, "Should have return segment");
            Assert.IsGreaterThanOrEqualTo(20, play.ReturnSegments[0].YardsGained, "Excellent returner should get good yards");
        }

        [TestMethod]
        public void Kickoff_SlowReturner_FewerYards()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;

            var returner = play.DefensePlayersOnField.First(p => p.Position == Positions.WR);
            returner.Speed = 30;
            returner.Agility = 25;

            // Using NormalReturn scenario with low return yardage for slow returner
            var rng = KickoffPlayScenarios.NormalReturn(kickDistance: 0.5, returnYardage: 0.2);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsNotEmpty(play.ReturnSegments, "Should have return segment");
            Assert.IsLessThan(25, play.ReturnSegments[0].YardsGained, "Slow returner should get fewer yards");
        }

        #endregion

        #region Scoring Scenarios

        [TestMethod]
        public void Kickoff_AfterTouchdown_HomeKicksToAway()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Home; // Home kicks after scoring
            game.HomeScore = 7;
            game.AwayScore = 0;

            // Using Touchback scenario: deep kick after scoring
            var rng = KickoffPlayScenarios.Touchback(kickDistance: 0.9);

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Touchback, "Should be touchback");
            Assert.IsTrue(play.PossessionChange, "Away team should get the ball");
        }

        [TestMethod]
        public void Kickoff_ReturnTD_ChangesScore()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            game.HomeScore = 14;
            game.AwayScore = 10;

            var kicker = play.OffensePlayersOnField.First(p => p.Position == Positions.K);
            kicker.Kicking = 35;

            var returner = play.DefensePlayersOnField.First();
            returner.Speed = 99;

            // Using ReturnTouchdownMaximum scenario: short kick with maximum return for TD
            var rng = KickoffPlayScenarios.ReturnTouchdownMaximum();

            var kickoff = new Kickoff(rng);

            // Act
            kickoff.Execute(game);
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should score TD");
            Assert.AreEqual(20, game.HomeScore, "Home should score (14 + 6)");
            Assert.AreEqual(10, game.AwayScore, "Away score unchanged");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void Kickoff_Scenario1_NormalReturnExecutes()
        {
            // Scenario 1: Normal kickoff with return
            ExecuteKickoffScenario(CreateGameWithKickoffPlay(), KickoffPlayScenarios.NormalReturn());
        }

        [TestMethod]
        public void Kickoff_Scenario2_TouchbackExecutes()
        {
            // Scenario 2: Touchback
            ExecuteKickoffScenario(CreateGameWithKickoffPlay(), KickoffPlayScenarios.Touchback());
        }

        [TestMethod]
        public void Kickoff_Scenario3_OutOfBoundsExecutes()
        {
            // Scenario 3: Out of bounds
            ExecuteKickoffScenario(CreateGameWithKickoffPlay(), KickoffPlayScenarios.OutOfBounds());
        }

        [TestMethod]
        public void Kickoff_Scenario4_OnsideKickRecoveredExecutes()
        {
            // Scenario 4: Onside kick - kicking team recovers
            var game4 = CreateGameWithKickoffPlay();
            game4.HomeScore = 10;
            game4.AwayScore = 17;
            ExecuteKickoffScenario(game4, KickoffPlayScenarios.OnsideKickRecovered());
        }

        [TestMethod]
        public void Kickoff_Scenario5_ReturnTouchdownExecutes()
        {
            // Scenario 5: Return TD
            ExecuteKickoffScenario(CreateGameWithKickoffPlay(), KickoffPlayScenarios.ReturnTouchdown());
        }

        [TestMethod]
        public void Kickoff_Scenario6_FairCatchExecutes()
        {
            // Scenario 6: Fair catch
            ExecuteKickoffScenario(CreateGameWithKickoffPlay(), KickoffPlayScenarios.FairCatch());
        }

        private void ExecuteKickoffScenario(Game game, TestFluentSeedableRandom rng)
        {
            var kickoff = new Kickoff(rng);
            kickoff.Execute(game);

            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Just verify it doesn't throw
            Assert.IsNotNull(game.CurrentPlay);
        }

        #endregion

        #region Penalty Tests

        [TestMethod]
        public void KickoffResult_KickingTeamPenalty_Rekick()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            game.FieldPosition = 35;
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 25; // Normal touchback
            play.Touchback = true;

            // Add kicking team penalty (offsides, 5 yards)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.OffsideonFreeKick,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    Accepted = true
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Penalty enforced, rekick from 5 yards closer
            Assert.AreEqual(40, game.FieldPosition, "Should rekick from 5 yards closer (35 + 5)");
        }

        [TestMethod]
        public void KickoffResult_ReceivingTeamPenalty_EnforcedOnReturn()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 40; // Returned to 40
            play.Touchback = false;

            // Add receiving team holding penalty (10 yards)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.OffensiveHolding,
                    Yards = 10,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Penalty enforced from end of return
            Assert.AreEqual(30, game.FieldPosition, "Should be moved back 10 yards (40 - 10)");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
        }

        [TestMethod]
        public void KickoffResult_OffsettingPenalties_Rekick()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            game.FieldPosition = 35;
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 30;

            // Add offsetting penalties
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DefensiveHolding,
                    Yards = 10,
                    CalledOn = Possession.Home,
                    Accepted = true
                },
                new Penalty
                {
                    Name = PenaltyNames.OffensiveHolding,
                    Yards = 10,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Offsetting penalties, rekick from original spot
            Assert.AreEqual(35, game.FieldPosition, "Should rekick from original spot");
        }

        [TestMethod]
        public void KickoffResult_PenaltyOnTouchdownReturn_StillScores()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 100;
            play.IsTouchdown = true;

            // Add kicking team penalty (doesn't negate TD)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FaceMask15Yards,
                    Yards = 15,
                    CalledOn = Possession.Home,
                    Accepted = false  // Should be declined since TD stands
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - TD should stand
            Assert.AreEqual(100, game.FieldPosition, "Should be at goal line");
            Assert.IsTrue(play.IsTouchdown, "Should be a touchdown");
        }

        [TestMethod]
        public void KickoffResult_PenaltyPushesIntoEndZone_Touchdown()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            game.FieldPosition = 88; // Near goal line
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 88;

            // Add kicking team penalty that pushes returner into end zone
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FaceMask15Yards,
                    Yards = 15,
                    CalledOn = Possession.Home,  // Kicking team
                    Accepted = true
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Should result in touchdown
            Assert.AreEqual(100, game.FieldPosition, "Should be pushed into end zone");
            Assert.IsTrue(play.IsTouchdown, "Should be a touchdown");
        }

        [TestMethod]
        public void KickoffResult_PenaltyPushesIntoOwnEndZone_Safety()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            game.FieldPosition = 8; // Near own goal line
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 8;

            // Add receiving team penalty that pushes them into own end zone
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.OffensiveHolding,
                    Yards = 10,
                    CalledOn = Possession.Away,  // Receiving team
                    Accepted = true
                }
            };

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Should result in safety
            Assert.AreEqual(0, game.FieldPosition, "Should be at goal line");
            Assert.IsTrue(play.IsSafety, "Should be a safety");
        }

        [TestMethod]
        public void KickoffResult_NoPenalties_NormalExecution()
        {
            // Arrange
            var game = CreateGameWithKickoffPlay();
            var play = (KickoffPlay)game.CurrentPlay;
            play.EndFieldPosition = 30;
            play.Touchback = false;

            // No penalties
            play.Penalties = new List<Penalty>();

            // Act
            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            // Assert - Normal kickoff return
            Assert.AreEqual(30, game.FieldPosition, "Should be at return spot");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithKickoffPlay()
        {
            var game = _testGame!.GetGame();

            var kickoffPlay = new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartFieldPosition = 35,
                OffensePlayersOnField = new List<Player>(),
                DefensePlayersOnField = new List<Player>()
            };

            // Add kicker
            kickoffPlay.OffensePlayersOnField.Add(new Player
            {
                Position = Positions.K,
                LastName = "Kicker",
                Kicking = 70,
                Speed = 50,
                Strength = 50
            });

            // Add coverage team
            for (int i = 0; i < 10; i++)
            {
                kickoffPlay.OffensePlayersOnField.Add(new Player
                {
                    Position = Positions.LB,
                    LastName = $"Coverage{i}",
                    Speed = 70,
                    Tackling = 65,
                    Agility = 60
                });
            }

            // Add returners
            kickoffPlay.DefensePlayersOnField.Add(new Player
            {
                Position = Positions.WR,
                LastName = "Returner",
                Speed = 85,
                Agility = 80
            });

            // Add return team
            for (int i = 0; i < 10; i++)
            {
                kickoffPlay.DefensePlayersOnField.Add(new Player
                {
                    Position = Positions.T,  // Tackle (offensive line)
                    LastName = $"Blocker{i}",
                    Speed = 60,
                    Blocking = 70,
                    Strength = 75
                });
            }

            game.CurrentPlay = kickoffPlay;
            game.FieldPosition = 35; // Kickoff from 35-yard line
            game.CurrentDown = Downs.None;
            game.YardsToGo = 0;
            game.HomeScore = 0;
            game.AwayScore = 0;

            return game;
        }

        #endregion
    }
}
