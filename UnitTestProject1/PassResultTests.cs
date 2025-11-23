using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PassResultTests
    {
        private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Touchdown Tests

        [TestMethod]
        public void PassResult_Touchdown_Awards6Points_HomePossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 88;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 15; // Will exceed 100
            passPlay.Possession = Possession.Home;
            var initialHomeScore = game.HomeScore;

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.IsTouchdown, "Should be marked as touchdown");
            Assert.AreEqual(100, passPlay.EndFieldPosition, "End position should be 100");
            Assert.AreEqual(initialHomeScore + 6, game.HomeScore, "Home team should get 6 points");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change after touchdown");
        }

        [TestMethod]
        public void PassResult_Touchdown_Awards6Points_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 85;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 20; // Will exceed 100
            passPlay.Possession = Possession.Away;
            var initialAwayScore = game.AwayScore;

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.IsTouchdown, "Should be marked as touchdown");
            Assert.AreEqual(100, passPlay.EndFieldPosition, "End position should be 100");
            Assert.AreEqual(initialAwayScore + 6, game.AwayScore, "Away team should get 6 points");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change after touchdown");
        }

        #endregion

        #region Safety Tests (Sacked in Own End Zone)

        [TestMethod]
        public void PassResult_SafetyOnSack_Awards2PointsToDefense_HomePossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 5;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = -8; // Sacked in own end zone
            passPlay.Possession = Possession.Home;
            var initialAwayScore = game.AwayScore;

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(0, passPlay.EndFieldPosition, "End position should be 0");
            Assert.AreEqual(initialAwayScore + 2, game.AwayScore, "Defense (Away) should get 2 points");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change after safety");
        }

        [TestMethod]
        public void PassResult_SafetyOnSack_Awards2PointsToDefense_AwayPossession()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 3;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = -5; // Sacked in own end zone
            passPlay.Possession = Possession.Away;
            var initialHomeScore = game.HomeScore;

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(0, passPlay.EndFieldPosition, "End position should be 0");
            Assert.AreEqual(initialHomeScore + 2, game.HomeScore, "Defense (Home) should get 2 points");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change after safety");
        }

        #endregion

        #region First Down Tests

        [TestMethod]
        public void PassResult_CompletedPassForFirstDown_ResetsDownAndDistance()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 15; // Exceeds yards to go

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(45, game.FieldPosition, "Field position should advance to 45");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
            Assert.IsFalse(passPlay.PossessionChange, "Possession should NOT change on first down");
        }

        [TestMethod]
        public void PassResult_ExactlyEnoughForFirstDown_ResetsDownAndDistance()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 40;
            game.YardsToGo = 8;
            game.CurrentDown = Downs.Third;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 8; // Exactly enough

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(48, game.FieldPosition, "Field position should advance to 48");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to first down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
        }

        #endregion

        #region Down Advancement Tests

        [TestMethod]
        public void PassResult_InsufficientYards_AdvancesDown()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 5; // Not enough for first down

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(30, game.FieldPosition, "Field position should advance to 30");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Should advance to second down");
            Assert.AreEqual(5, game.YardsToGo, "Should have 5 yards to go");
            Assert.IsFalse(passPlay.PossessionChange, "Possession should NOT change");
        }

        [TestMethod]
        public void PassResult_IncompletePass_AdvancesDownNoYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 35;
            game.YardsToGo = 7;
            game.CurrentDown = Downs.Second;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 0; // Incomplete

            // Create an incomplete pass segment
            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var receiver = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            passPlay.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = false,
                AirYards = 0,
                YardsAfterCatch = 0
            });

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(35, game.FieldPosition, "Field position should NOT change on incomplete");
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Should advance to third down");
            Assert.AreEqual(7, game.YardsToGo, "Should still need 7 yards");
        }

        [TestMethod]
        public void PassResult_Sack_AdvancesDownWithNegativeYards()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = -6; // Sacked for 6 yards

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(24, game.FieldPosition, "Field position should move back to 24");
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Should advance to second down");
            Assert.AreEqual(16, game.YardsToGo, "Should need 16 yards (10 + 6 lost)");
        }

        #endregion

        #region Turnover on Downs Tests

        [TestMethod]
        public void PassResult_FourthDownFailure_TurnoverOnDowns()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 50;
            game.CurrentDown = Downs.Fourth;
            game.YardsToGo = 8;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 5; // Not enough

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(55, game.FieldPosition, "Field position should advance");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to First down after turnover");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go after turnover");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change on turnover");
        }

        [TestMethod]
        public void PassResult_FourthDownIncomplete_TurnoverOnDowns()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 45;
            game.CurrentDown = Downs.Fourth;
            game.YardsToGo = 3;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 0; // Incomplete

            // Create incomplete segment
            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var receiver = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            passPlay.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = false,
                AirYards = 0,
                YardsAfterCatch = 0
            });

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(45, game.FieldPosition, "Field position should NOT change");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to First down after turnover");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go after turnover");
            Assert.IsTrue(passPlay.PossessionChange, "Possession should change on turnover");
        }

        #endregion

        #region Field Position Update Tests

        [TestMethod]
        public void PassResult_CompletedPass_UpdatesStartAndEndFieldPosition()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 40;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 12;

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(40, passPlay.StartFieldPosition, "Start position should be set to 40");
            Assert.AreEqual(52, passPlay.EndFieldPosition, "End position should be 52");
            Assert.AreEqual(52, game.FieldPosition, "Game field position should be updated to 52");
        }

        [TestMethod]
        public void PassResult_IncompletePass_NoFieldPositionChange()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 35;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 0;

            // Create incomplete segment
            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var receiver = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            passPlay.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = false,
                AirYards = 0,
                YardsAfterCatch = 0
            });

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(35, passPlay.StartFieldPosition, "Start position should be 35");
            Assert.AreEqual(35, passPlay.EndFieldPosition, "End position should be 35 (no change)");
            Assert.AreEqual(35, game.FieldPosition, "Game field position should remain 35");
        }

        [TestMethod]
        public void PassResult_Sack_NegativeFieldPositionChange()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 40;
            game.CurrentDown = Downs.Second;
            game.YardsToGo = 8;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = -7; // Sacked for 7

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(40, passPlay.StartFieldPosition, "Start position should be 40");
            Assert.AreEqual(33, passPlay.EndFieldPosition, "End position should be 33 (lost 7)");
            Assert.AreEqual(33, game.FieldPosition, "Game field position should be 33");
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Should advance down");
            Assert.AreEqual(15, game.YardsToGo, "Should need 15 yards after losing 7");
        }

        #endregion

        #region Pass Segment Stats Tests

        [TestMethod]
        public void PassResult_WithPassSegment_LogsCorrectStats()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 30;
            var passPlay = (PassPlay)game.CurrentPlay!;
            passPlay.YardsGained = 15;

            // Create completed pass segment
            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var receiver = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            passPlay.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = true,
                Type = PassType.Forward,
                AirYards = 10,
                YardsAfterCatch = 5
            });

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(45, game.FieldPosition, "Should advance field position");
            Assert.IsNotNull(passPlay.PassSegments[0].Passer, "Passer should be recorded");
            Assert.IsNotNull(passPlay.PassSegments[0].Receiver, "Receiver should be recorded");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void PassResult_MixedPlays_TracksCorrectly()
        {
            // Arrange - simulate a drive with different pass results
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 20;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            // First down: complete pass for 6 yards
            var passPlay1 = (PassPlay)game.CurrentPlay!;
            passPlay1.YardsGained = 6;
            var passResult1 = new PassResult();
            passResult1.Execute(game);

            // Assert after first play
            Assert.AreEqual(26, game.FieldPosition);
            Assert.AreEqual(Downs.Second, game.CurrentDown);
            Assert.AreEqual(4, game.YardsToGo);

            // Second down: incomplete pass
            var passPlay2 = new PassPlay { YardsGained = 0, Possession = Possession.Home };
            var qb = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0];
            var receiver = _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0];
            passPlay2.PassSegments.Add(new PassSegment
            {
                Passer = qb,
                Receiver = receiver,
                IsComplete = false,
                AirYards = 0,
                YardsAfterCatch = 0
            });
            game.CurrentPlay = passPlay2;
            var passResult2 = new PassResult();
            passResult2.Execute(game);

            // Assert after second play
            Assert.AreEqual(26, game.FieldPosition, "No change on incomplete");
            Assert.AreEqual(Downs.Third, game.CurrentDown);
            Assert.AreEqual(4, game.YardsToGo);

            // Third down: complete pass for 12 yards (converts to first down)
            var passPlay3 = new PassPlay { YardsGained = 12, Possession = Possession.Home };
            game.CurrentPlay = passPlay3;
            var passResult3 = new PassResult();
            passResult3.Execute(game);

            // Assert after third play - should be new first down
            Assert.AreEqual(38, game.FieldPosition);
            Assert.AreEqual(Downs.First, game.CurrentDown);
            Assert.AreEqual(10, game.YardsToGo);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();

            // Create a pass play
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 0,
                YardsGained = 0
            };

            game.CurrentPlay = passPlay;
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
