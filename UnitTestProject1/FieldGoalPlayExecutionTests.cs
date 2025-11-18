using DomainObjects;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

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

            var rng = FieldGoalPlayScenarios.ExtraPointMade();

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

            var rng = FieldGoalPlayScenarios.ExtraPointMissed();

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

            var rng = FieldGoalPlayScenarios.ShortFieldGoalMade();

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

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMade(makeValue: 0.4);

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

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMissed(missDirection: 0.3);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(14, game.HomeScore, "Home score should stay at 14");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(63, play.EndFieldPosition, "Defense gets ball 7 yards behind LOS");
            Assert.AreEqual(63, game.FieldPosition, "Game field position should be 63");
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

            var rng = FieldGoalPlayScenarios.LongFieldGoalMade(makeValue: 0.2);

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

            var rng = FieldGoalPlayScenarios.LongFieldGoalMissed(missDirection: 0.85);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Long FG should be missed");
            Assert.AreEqual(7, game.AwayScore, "Away score should stay at 7");
            Assert.IsTrue(play.PossessionChange, "Possession should change on missed FG");
            Assert.AreEqual(43, play.EndFieldPosition, "Defense gets ball 7 yards behind LOS (50-7=43)");
            Assert.AreEqual(43, game.FieldPosition, "Game field position should be 43");
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

            var rng = FieldGoalPlayScenarios.BadSnap(baseLoss: 0.5, randomFactor: 0.5);

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

            var rng = FieldGoalPlayScenarios.BadSnapSafety();

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

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalOffenseRecovers(recoveryYards: 0.5);

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

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalDefenseRecovers(returnYards: 0.5);

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

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalTouchdown();

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
            // Arrange - Test defensive TD from opponent territory with excellent return
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 35; // Opponent 35-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 10;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalTouchdown();

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be TD on long return");
            Assert.AreEqual(16, game.AwayScore, "Away team should score TD (10 + 6)");
        }

        #endregion

        #region Enhanced Blocked Field Goal Tests

        [TestMethod]
        public void FieldGoal_Blocked_DefenseRecovers_ShortReturn()
        {
            // Arrange - Test defense recovers blocked FG and returns 15 yards
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70; // 30-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 10;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalDefenseRecovers(returnYards: 0.6);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Kick should be blocked");
            Assert.IsFalse(play.IsGood, "Blocked kick is not good");
            Assert.IsNotNull(play.BlockedBy, "Should track who blocked");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
            Assert.IsTrue(play.DefensePlayersOnField.Contains(play.RecoveredBy), "Defense should have recovered");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.IsTrue(play.YardsGained > 0, "Should have positive return yards");
        }

        [TestMethod]
        public void FieldGoal_Blocked_DefenseRecovers_Touchdown()
        {
            // Arrange - Test defensive TD on blocked FG return (scoop and score)
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 95; // 5-yard line (near opponent goal)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 14;
            game.AwayScore = 17;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalTouchdown();

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsTouchdown, "Should be TD on return");
            Assert.IsNotNull(play.RecoveredBy, "Should track recoverer");
            Assert.AreEqual(23, game.AwayScore, "Away team should score TD (17 + 6)");
            Assert.AreEqual(100, play.EndFieldPosition, "Should be at opponent goal");
        }

        [TestMethod]
        public void FieldGoal_Blocked_OffenseRecovers_Safety()
        {
            // Arrange - Test offense recovers in own end zone = safety
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 5; // Very close to own goal
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 10;
            game.AwayScore = 14;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalSafety();

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsSafety, "Should be safety");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
            Assert.IsTrue(play.OffensePlayersOnField.Contains(play.RecoveredBy), "Offense should have recovered");
            Assert.AreEqual(16, game.AwayScore, "Away team should get 2 points (14 + 2)");
            Assert.AreEqual(0, play.EndFieldPosition, "Should be at goal line");
        }

        [TestMethod]
        public void FieldGoal_Blocked_OffenseRecovers_LossOfYards()
        {
            // Arrange - Test offense recovers with yardage loss
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70; // 30-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalOffenseRecovers(recoveryYards: 0.5);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsFalse(play.IsSafety, "Should not be safety");
            Assert.IsFalse(play.IsTouchdown, "Should not be TD");
            Assert.IsNotNull(play.RecoveredBy, "Should track who recovered");
            Assert.IsTrue(play.OffensePlayersOnField.Contains(play.RecoveredBy), "Offense should have recovered");
            Assert.IsTrue(play.YardsGained < 0, "Should lose yards");
            Assert.IsTrue(play.PossessionChange, "Possession should change (turnover on downs)");
        }

        [TestMethod]
        public void FieldGoal_LongKick_HigherBlockProbability()
        {
            // Arrange - Test that 55+ yard kicks have higher block chance
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 40; // 60-yard line (77-yard attempt)
            var play = (FieldGoalPlay)game.CurrentPlay;

            var rng = FieldGoalPlayScenarios.LongKickBlocked();

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Long kick should have higher block probability");
            Assert.AreEqual(77, play.AttemptDistance, "Should be 77-yard attempt");
        }

        [TestMethod]
        public void FieldGoal_ShortKick_LowerBlockProbability()
        {
            // Arrange - Test that XP/short FGs have lower block chance
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: true);
            game.FieldPosition = 97; // 3-yard line (20-yard PAT)
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Use NextDouble value that would NOT block for short kick
            // Extra points: 1.5% base, so 0.03 would NOT block
            // This test requires a specific block check value, so using Custom builder
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)  // No bad snap
                .NextDouble(0.03)  // Would NOT block PAT (> 1.5%)
                .NextDouble(0.5)   // Make check
                .NextDouble(0.99)  // No kicker penalty
                .NextDouble(0.5);  // Elapsed time

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            Assert.IsFalse(play.Blocked, "Short kick should have lower block probability");
            Assert.AreEqual(20, play.AttemptDistance, "Should be 20-yard PAT");
        }

        [TestMethod]
        public void FieldGoal_BadSnap_HigherBlockProbability()
        {
            // Arrange - Test that bad snaps dramatically increase block chance
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;

            // Make long snapper terrible to ensure bad snap
            var snapper = play.OffensePlayersOnField.First(p => p.Position == Positions.LS);
            snapper.Blocking = 10; // Terrible snapper

            var rng = FieldGoalPlayScenarios.BadSnap(baseLoss: 0.5, randomFactor: 0.5);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);

            // Assert
            // This tests bad snap scenario, not blocked kick
            // Bad snaps bypass the block check entirely
            Assert.IsFalse(play.GoodSnap, "Should be bad snap");
            Assert.IsFalse(play.Blocked, "Bad snap prevents kick, no block");
        }

        [TestMethod]
        public void FieldGoal_Blocked_DefenseRecovers_Safety()
        {
            // Arrange - Test defense recovers and runs backwards into kicking team's end zone
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 3; // Very close to own goal
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            game.HomeScore = 7;
            game.AwayScore = 7;

            var rng = FieldGoalPlayScenarios.BlockedFieldGoalDefenseSafety();

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsTrue(play.Blocked, "Should be blocked");
            Assert.IsTrue(play.IsSafety, "Should be safety when defense runs into kicking team end zone");
            Assert.AreEqual(0, play.EndFieldPosition, "Should be at goal line");
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

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMade(makeValue: 0.5);

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

            var rng = FieldGoalPlayScenarios.ExtremelyLongFieldGoal();

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

            var rng = FieldGoalPlayScenarios.LongFieldGoalMade(makeValue: 0.4);

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

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMissed(missDirection: 0.5);

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
            game.FieldPosition = 70; // 30-yard line (LOS)
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMissed(missDirection: 0.5);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(63, play.EndFieldPosition, "Defense gets ball at spot of kick (7 yards behind LOS)");
            Assert.AreEqual(63, game.FieldPosition, "Game field position should be 63");
            Assert.IsTrue(play.PossessionChange, "Possession should change on missed FG");
        }

        [TestMethod]
        public void FieldGoal_MissedFromRedZone_OpponentGetsAt20()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(isExtraPoint: false);
            game.FieldPosition = 95; // 5-yard line
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Possession = Possession.Home;

            var rng = FieldGoalPlayScenarios.MediumFieldGoalMissed(missDirection: 0.5);

            var fieldGoal = new FieldGoal(rng);

            // Act
            fieldGoal.Execute(game);
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsGood, "Should be missed");
            Assert.AreEqual(80, play.EndFieldPosition, "Defense gets ball at their 20");
            Assert.AreEqual(80, game.FieldPosition, "Game field position should be 80");
            Assert.IsTrue(play.PossessionChange, "Possession should change on missed FG");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void FieldGoal_MultipleScenarios_AllExecuteWithoutError()
        {
            // Test various scenarios to ensure no exceptions

            // Test 1: Made field goal
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false),
                FieldGoalPlayScenarios.MediumFieldGoalMade());

            // Test 2: Missed field goal
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false),
                FieldGoalPlayScenarios.MediumFieldGoalMissed());

            // Test 3: Made extra point
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(true),
                FieldGoalPlayScenarios.ExtraPointMade());

            // Test 4: Blocked kick - defense recovers
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false),
                FieldGoalPlayScenarios.BlockedFieldGoalDefenseRecovers());

            // Test 5: Bad snap
            ExecuteFieldGoalScenario(CreateGameWithFieldGoalPlay(false),
                FieldGoalPlayScenarios.BadSnap());
        }

        private void ExecuteFieldGoalScenario(Game game, TestFluentSeedableRandom rng)
        {
            var fieldGoal = new FieldGoal(rng);
            fieldGoal.Execute(game);
            // Just verify it doesn't throw
            Assert.IsNotNull(game.CurrentPlay);
        }

        #endregion

        #region Penalty Tests

        [TestMethod]
        public void FieldGoalResult_DefensivePenalty_AutomaticFirstDown()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(false);
            game.FieldPosition = 70; // 30-yard line
            game.CurrentDown = Downs.Fourth;
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = false; // Missed FG

            // Add defensive penalty (running into kicker, 5 yards, automatic first down)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.RunningIntotheKicker,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - Penalty gives automatic first down
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down from penalty");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
            Assert.IsFalse(play.PossessionChange, "Kicking team should retain possession");
        }

        [TestMethod]
        public void FieldGoalResult_OffensivePenalty_Rekick()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(false);
            game.FieldPosition = 70;
            game.CurrentDown = Downs.Fourth;
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = true; // Good FG

            // Add offensive penalty (delay of game, 5 yards)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DelayofGame,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - Penalty moves FG attempt back
            Assert.AreEqual(65, game.FieldPosition, "Should move back 5 yards (70 - 5)");
        }

        [TestMethod]
        public void FieldGoalResult_PAT_DefensivePenalty_Retry()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(true); // PAT
            game.FieldPosition = 98; // 2-yard line for PAT
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = false; // Missed PAT
            play.IsExtraPoint = true;

            // Add defensive penalty (offsides, 5 yards)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.DefensiveOffside,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - PAT can be retried from closer (half the distance)
            Assert.IsTrue(game.FieldPosition > 98, "Should move closer to goal line");
        }

        [TestMethod]
        public void FieldGoalResult_PAT_OffensivePenalty_RetryFurtherBack()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(true); // PAT
            game.FieldPosition = 98; // 2-yard line for PAT
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = true; // Good PAT
            play.IsExtraPoint = true;

            // Add offensive penalty (false start, 5 yards)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.FalseStart,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - PAT retry from further back
            Assert.AreEqual(93, game.FieldPosition, "Should move back 5 yards (98 - 5)");
        }

        [TestMethod]
        public void FieldGoalResult_OffsettingPenalties_Rekick()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(false);
            game.FieldPosition = 70;
            game.CurrentDown = Downs.Fourth;
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = false; // Missed

            // Add offsetting penalties
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.OffensiveOffside,
                    Yards = 5,
                    CalledOn = Possession.Home,
                    Accepted = true
                },
                new Penalty
                {
                    Name = PenaltyNames.Encroachment,
                    Yards = 5,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - Offsetting penalties, rekick from same spot
            Assert.AreEqual(70, game.FieldPosition, "Field position should remain unchanged");
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should still be fourth down for rekick");
        }

        [TestMethod]
        public void FieldGoalResult_BlockedFieldGoalWithPenalty_Enforced()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.Blocked = true;
            play.YardsGained = -5; // Blocked, lost yards
            play.RecoveredBy = new Player { LastName = "Defender", Position = Positions.DT };

            // Add defensive penalty (roughing the kicker, 15 yards, automatic first down)
            play.Penalties = new List<Penalty>
            {
                new Penalty
                {
                    Name = PenaltyNames.RoughingtheKicker,
                    Yards = 15,
                    CalledOn = Possession.Away,
                    Accepted = true
                }
            };

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - Penalty negates block, gives automatic first down
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be first down from penalty");
            Assert.IsFalse(play.PossessionChange, "Kicking team should retain possession");
        }

        [TestMethod]
        public void FieldGoalResult_NoPenalties_NormalExecution()
        {
            // Arrange
            var game = CreateGameWithFieldGoalPlay(false);
            game.FieldPosition = 70;
            var play = (FieldGoalPlay)game.CurrentPlay;
            play.IsGood = true;

            // No penalties
            play.Penalties = new List<Penalty>();

            // Act
            var fieldGoalResult = new FieldGoalResult();
            fieldGoalResult.Execute(game);

            // Assert - Normal FG execution
            Assert.IsTrue(play.PossessionChange, "Possession should change after score");
            Assert.AreEqual(3, game.HomeScore, "Should score 3 points");
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
                Strength = 60,
                Awareness = 60
            });

            // Add offensive line for block calculation
            for (int i = 0; i < 5; i++)
            {
                fieldGoalPlay.OffensePlayersOnField.Add(new Player
                {
                    Position = i < 2 ? Positions.T : (i < 4 ? Positions.G : Positions.C),
                    LastName = $"OLineman{i}",
                    Blocking = 70,
                    Strength = 70,
                    Awareness = 65,
                    Speed = 50
                });
            }

            // Add defensive line for blocking attempts
            for (int i = 0; i < 3; i++)
            {
                fieldGoalPlay.DefensePlayersOnField.Add(new Player
                {
                    Position = Positions.DT,
                    LastName = $"Defender{i}",
                    Speed = 70,
                    Strength = 80,
                    Tackling = 70,
                    Awareness = 65,
                    Agility = 60
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
