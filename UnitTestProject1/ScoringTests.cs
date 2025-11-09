using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class ScoringTests
    {
        private readonly TestGame _testGame = new TestGame();

        #region AddTouchdown Tests

        [TestMethod]
        public void AddTouchdown_HomeTeam_Adds6Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act
            game.AddTouchdown(Possession.Home);

            // Assert
            Assert.AreEqual(6, game.HomeScore, "Home team should have 6 points");
            Assert.AreEqual(0, game.AwayScore, "Away team should still have 0 points");
        }

        [TestMethod]
        public void AddTouchdown_AwayTeam_Adds6Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act
            game.AddTouchdown(Possession.Away);

            // Assert
            Assert.AreEqual(0, game.HomeScore, "Home team should still have 0 points");
            Assert.AreEqual(6, game.AwayScore, "Away team should have 6 points");
        }

        [TestMethod]
        public void AddTouchdown_MultipleTouchdowns_AccumulatesPoints()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 7;
            game.AwayScore = 3;

            // Act
            game.AddTouchdown(Possession.Home); // Home: 7 + 6 = 13
            game.AddTouchdown(Possession.Away); // Away: 3 + 6 = 9
            game.AddTouchdown(Possession.Home); // Home: 13 + 6 = 19

            // Assert
            Assert.AreEqual(19, game.HomeScore, "Home team should have 19 points");
            Assert.AreEqual(9, game.AwayScore, "Away team should have 9 points");
        }

        [TestMethod]
        public void AddTouchdown_PossessionNone_DoesNotAddPoints()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 10;
            game.AwayScore = 7;

            // Act
            game.AddTouchdown(Possession.None);

            // Assert
            Assert.AreEqual(10, game.HomeScore, "Home score should not change");
            Assert.AreEqual(7, game.AwayScore, "Away score should not change");
        }

        #endregion

        #region AddFieldGoal Tests

        [TestMethod]
        public void AddFieldGoal_HomeTeam_Adds3Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act
            game.AddFieldGoal(Possession.Home);

            // Assert
            Assert.AreEqual(3, game.HomeScore, "Home team should have 3 points");
            Assert.AreEqual(0, game.AwayScore, "Away team should still have 0 points");
        }

        [TestMethod]
        public void AddFieldGoal_AwayTeam_Adds3Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act
            game.AddFieldGoal(Possession.Away);

            // Assert
            Assert.AreEqual(0, game.HomeScore, "Home team should still have 0 points");
            Assert.AreEqual(3, game.AwayScore, "Away team should have 3 points");
        }

        [TestMethod]
        public void AddFieldGoal_MultipleFieldGoals_AccumulatesPoints()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 10;

            // Act
            game.AddFieldGoal(Possession.Home); // Home: 14 + 3 = 17
            game.AddFieldGoal(Possession.Away); // Away: 10 + 3 = 13
            game.AddFieldGoal(Possession.Away); // Away: 13 + 3 = 16

            // Assert
            Assert.AreEqual(17, game.HomeScore, "Home team should have 17 points");
            Assert.AreEqual(16, game.AwayScore, "Away team should have 16 points");
        }

        #endregion

        #region AddSafety Tests

        [TestMethod]
        public void AddSafety_HomeTeamOnDefense_Adds2Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 7;
            game.AwayScore = 14;

            // Act - Home team forced a safety (they were on defense)
            game.AddSafety(Possession.Home);

            // Assert
            Assert.AreEqual(9, game.HomeScore, "Home team should have 9 points (7 + 2)");
            Assert.AreEqual(14, game.AwayScore, "Away team score should not change");
        }

        [TestMethod]
        public void AddSafety_AwayTeamOnDefense_Adds2Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 10;
            game.AwayScore = 3;

            // Act - Away team forced a safety (they were on defense)
            game.AddSafety(Possession.Away);

            // Assert
            Assert.AreEqual(10, game.HomeScore, "Home team score should not change");
            Assert.AreEqual(5, game.AwayScore, "Away team should have 5 points (3 + 2)");
        }

        [TestMethod]
        public void AddSafety_MultipleSafeties_AccumulatesPoints()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 6;
            game.AwayScore = 7;

            // Act
            game.AddSafety(Possession.Home); // Home: 6 + 2 = 8
            game.AddSafety(Possession.Home); // Home: 8 + 2 = 10

            // Assert
            Assert.AreEqual(10, game.HomeScore, "Home team should have 10 points");
            Assert.AreEqual(7, game.AwayScore, "Away team score should not change");
        }

        #endregion

        #region AddExtraPoint Tests

        [TestMethod]
        public void AddExtraPoint_HomeTeam_Adds1Point()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 6; // After TD
            game.AwayScore = 0;

            // Act
            game.AddExtraPoint(Possession.Home);

            // Assert
            Assert.AreEqual(7, game.HomeScore, "Home team should have 7 points (6 + 1)");
            Assert.AreEqual(0, game.AwayScore, "Away team should still have 0 points");
        }

        [TestMethod]
        public void AddExtraPoint_AwayTeam_Adds1Point()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 7;
            game.AwayScore = 6; // After TD

            // Act
            game.AddExtraPoint(Possession.Away);

            // Assert
            Assert.AreEqual(7, game.HomeScore, "Home team score should not change");
            Assert.AreEqual(7, game.AwayScore, "Away team should have 7 points (6 + 1)");
        }

        #endregion

        #region AddTwoPointConversion Tests

        [TestMethod]
        public void AddTwoPointConversion_HomeTeam_Adds2Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 6; // After TD
            game.AwayScore = 0;

            // Act
            game.AddTwoPointConversion(Possession.Home);

            // Assert
            Assert.AreEqual(8, game.HomeScore, "Home team should have 8 points (6 + 2)");
            Assert.AreEqual(0, game.AwayScore, "Away team should still have 0 points");
        }

        [TestMethod]
        public void AddTwoPointConversion_AwayTeam_Adds2Points()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 6; // After TD

            // Act
            game.AddTwoPointConversion(Possession.Away);

            // Assert
            Assert.AreEqual(14, game.HomeScore, "Home team score should not change");
            Assert.AreEqual(8, game.AwayScore, "Away team should have 8 points (6 + 2)");
        }

        #endregion

        #region Mixed Scoring Scenarios

        [TestMethod]
        public void MixedScoring_RealisticGameScenario_TracksProperly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - Simulate a realistic scoring sequence
            game.AddTouchdown(Possession.Home);      // Home: 6
            game.AddExtraPoint(Possession.Home);     // Home: 7
            game.AddFieldGoal(Possession.Away);      // Away: 3
            game.AddTouchdown(Possession.Away);      // Away: 9
            game.AddTwoPointConversion(Possession.Away); // Away: 11
            game.AddSafety(Possession.Home);         // Home: 9
            game.AddTouchdown(Possession.Home);      // Home: 15
            game.AddExtraPoint(Possession.Home);     // Home: 16

            // Assert
            Assert.AreEqual(16, game.HomeScore, "Home should have 16 points");
            Assert.AreEqual(11, game.AwayScore, "Away should have 11 points");
        }

        [TestMethod]
        public void MixedScoring_CloseGame_TracksProperly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - Back-and-forth scoring
            game.AddFieldGoal(Possession.Home);      // Home: 3
            game.AddFieldGoal(Possession.Away);      // Away: 3
            game.AddTouchdown(Possession.Home);      // Home: 9
            game.AddExtraPoint(Possession.Home);     // Home: 10
            game.AddTouchdown(Possession.Away);      // Away: 9
            game.AddExtraPoint(Possession.Away);     // Away: 10
            game.AddFieldGoal(Possession.Home);      // Home: 13

            // Assert
            Assert.AreEqual(13, game.HomeScore, "Home should have 13 points");
            Assert.AreEqual(10, game.AwayScore, "Away should have 10 points");
        }

        [TestMethod]
        public void MixedScoring_HighScoringGame_TracksProperly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - High scoring game
            for (int i = 0; i < 4; i++)
            {
                game.AddTouchdown(Possession.Home);
                game.AddExtraPoint(Possession.Home);
                game.AddTouchdown(Possession.Away);
                game.AddExtraPoint(Possession.Away);
            }
            game.AddFieldGoal(Possession.Home); // Final field goal

            // Assert
            Assert.AreEqual(31, game.HomeScore, "Home should have 31 points (4 TDs + XPs + FG)");
            Assert.AreEqual(28, game.AwayScore, "Away should have 28 points (4 TDs + XPs)");
        }

        [TestMethod]
        public void AllScoringMethods_SingleTeam_CalculatesCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;

            // Act - Home team scores with all methods
            game.AddTouchdown(Possession.Home);         // 6
            game.AddExtraPoint(Possession.Home);        // 7
            game.AddFieldGoal(Possession.Home);         // 10
            game.AddSafety(Possession.Home);            // 12
            game.AddTouchdown(Possession.Home);         // 18
            game.AddTwoPointConversion(Possession.Home); // 20

            // Assert
            Assert.AreEqual(20, game.HomeScore, "Home should have 20 points");
        }

        #endregion

        #region Score Persistence Tests

        [TestMethod]
        public void Scoring_AfterMultipleOperations_PersistsCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();

            // Act
            game.AddTouchdown(Possession.Home);
            var scoreAfterTD = game.HomeScore;

            game.AddExtraPoint(Possession.Home);
            var scoreAfterXP = game.HomeScore;

            // Assert
            Assert.AreEqual(6, scoreAfterTD, "TD should add 6");
            Assert.AreEqual(7, scoreAfterXP, "XP should bring to 7");
            Assert.AreEqual(7, game.HomeScore, "Final score should be 7");
        }

        #endregion
    }
}
