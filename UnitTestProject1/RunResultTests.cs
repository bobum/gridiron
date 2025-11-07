using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class RunResultTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Touchdown Tests

        [TestMethod]
        public void RunResult_Touchdown_Awards6Points_HomePossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 95;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 10; // Will exceed 100
            runPlay.Possession = Possession.Home;
            var initialHomeScore = game.HomeScore;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.IsTouchdown, "Should be marked as touchdown");
            Assert.AreEqual(100, runPlay.EndFieldPosition, "End position should be 100");
            Assert.AreEqual(initialHomeScore + 6, game.HomeScore, "Home team should get 6 points");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change after touchdown");
        }

        [TestMethod]
        public void RunResult_Touchdown_Awards6Points_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 92;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 15; // Will exceed 100
            runPlay.Possession = Possession.Away;
            var initialAwayScore = game.AwayScore;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.IsTouchdown, "Should be marked as touchdown");
            Assert.AreEqual(100, runPlay.EndFieldPosition, "End position should be 100");
            Assert.AreEqual(initialAwayScore + 6, game.AwayScore, "Away team should get 6 points");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change after touchdown");
        }

        #endregion

        #region Safety Tests

        [TestMethod]
        public void RunResult_Safety_Awards2PointsToDefense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 3;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = -5; // Tackled in own end zone
            runPlay.Possession = Possession.Home;
            var initialAwayScore = game.AwayScore;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(0, runPlay.EndFieldPosition, "End position should be 0");
            Assert.AreEqual(initialAwayScore + 2, game.AwayScore, "Defense (Away) should get 2 points");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change after safety");
        }

        [TestMethod]
        public void RunResult_Safety_Awards2PointsToDefense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 2;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = -3; // Tackled in own end zone
            runPlay.Possession = Possession.Away;
            var initialHomeScore = game.HomeScore;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(0, runPlay.EndFieldPosition, "End position should be 0");
            Assert.AreEqual(initialHomeScore + 2, game.HomeScore, "Defense (Home) should get 2 points");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change after safety");
        }

        #endregion

        #region First Down Tests

        [TestMethod]
        public void RunResult_FirstDown_ResetsDownAndDistance()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 12; // Exceeds yards to go

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(37, game.FieldPosition, "Field position should advance to 37");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
            Assert.IsFalse(runPlay.PossessionChange, "Possession should NOT change on first down");
        }

        [TestMethod]
        public void RunResult_ExactlyEnoughForFirstDown_ResetsDownAndDistance()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 7;
            game.CurrentDown = Downs.Third;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 7; // Exactly enough

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(37, game.FieldPosition, "Field position should advance to 37");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
        }

        #endregion

        #region Down Advancement Tests

        [TestMethod]
        public void RunResult_InsufficientYards_AdvancesDown()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 4; // Not enough for first down

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(29, game.FieldPosition, "Field position should advance to 29");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Should advance to second down");
            Assert.AreEqual(6, game.YardsToGo, "Should have 6 yards to go");
            Assert.IsFalse(runPlay.PossessionChange, "Possession should NOT change");
        }

        [TestMethod]
        public void RunResult_SecondDown_AdvancesToThirdDown()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.CurrentDown = Downs.Second;
            game.YardsToGo = 7;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 3;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Should advance to third down");
            Assert.AreEqual(4, game.YardsToGo, "Should have 4 yards to go");
        }

        [TestMethod]
        public void RunResult_ThirdDown_AdvancesToFourthDown()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.CurrentDown = Downs.Third;
            game.YardsToGo = 8;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 2;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to fourth down");
            Assert.AreEqual(6, game.YardsToGo, "Should have 6 yards to go");
        }

        #endregion

        #region Turnover on Downs Tests

        [TestMethod]
        public void RunResult_FourthDownFailure_TurnoverOnDowns()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Fourth;
            game.YardsToGo = 5;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 3; // Not enough

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(43, game.FieldPosition, "Field position should advance");
            Assert.AreEqual(Downs.None, game.CurrentDown, "Should be set to None after turnover");
            Assert.IsTrue(runPlay.PossessionChange, "Possession should change on turnover");
        }

        #endregion

        #region Field Position Update Tests

        [TestMethod]
        public void RunResult_UpdatesStartAndEndFieldPosition()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 30;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = 8;

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(30, runPlay.StartFieldPosition, "Start position should be set to 30");
            Assert.AreEqual(38, runPlay.EndFieldPosition, "End position should be 38");
            Assert.AreEqual(38, game.FieldPosition, "Game field position should be updated to 38");
        }

        [TestMethod]
        public void RunResult_NegativeYards_UpdatesFieldPositionCorrectly()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 35;
            game.CurrentDown = Downs.Second;
            game.YardsToGo = 8;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.YardsGained = -3; // Loss of 3

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(35, runPlay.StartFieldPosition, "Start position should be 35");
            Assert.AreEqual(32, runPlay.EndFieldPosition, "End position should be 32 (lost 3)");
            Assert.AreEqual(32, game.FieldPosition, "Game field position should be 32");
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Should advance down");
            Assert.AreEqual(11, game.YardsToGo, "Should need 11 yards after losing 3");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void RunResult_MultipleDownsToFirstDown_TracksCorrectly()
        {
            // Arrange - simulate a drive
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 20;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            // First down: 4 yards
            var runPlay1 = (RunPlay)game.CurrentPlay;
            runPlay1.YardsGained = 4;
            var runResult1 = new RunResult();
            runResult1.Execute(game);

            // Assert after first play
            Assert.AreEqual(24, game.FieldPosition);
            Assert.AreEqual(Downs.Second, game.CurrentDown);
            Assert.AreEqual(6, game.YardsToGo);

            // Second down: 3 yards
            var runPlay2 = new RunPlay { YardsGained = 3, Possession = Possession.Home };
            game.CurrentPlay = runPlay2;
            var runResult2 = new RunResult();
            runResult2.Execute(game);

            // Assert after second play
            Assert.AreEqual(27, game.FieldPosition);
            Assert.AreEqual(Downs.Third, game.CurrentDown);
            Assert.AreEqual(3, game.YardsToGo);

            // Third down: 5 yards (converts to first down)
            var runPlay3 = new RunPlay { YardsGained = 5, Possession = Possession.Home };
            game.CurrentPlay = runPlay3;
            var runResult3 = new RunResult();
            runResult3.Execute(game);

            // Assert after third play - should be new first down
            Assert.AreEqual(32, game.FieldPosition);
            Assert.AreEqual(Downs.First, game.CurrentDown);
            Assert.AreEqual(10, game.YardsToGo);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithRunPlay()
        {
            var game = _testGame.GetGame();

            // Create a run play
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 0,
                YardsGained = 0
            };

            var rb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0];
            runPlay.RunSegments.Add(new RunSegment
            {
                BallCarrier = rb,
                YardsGained = 0,
                Direction = RunDirection.Middle,
                EndedInFumble = false
            });

            game.CurrentPlay = runPlay;
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            game.HomeScore = 0;
            game.AwayScore = 0;

            return game;
        }

        #endregion
    }
}
