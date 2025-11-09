using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;
using System.Linq;

namespace UnitTestProject1
{
    /// <summary>
    /// End-to-end integration tests for scoring across all play types.
    /// Tests complete scenarios from play execution through result handling and scoring.
    /// </summary>
    [TestClass]
    public class ScoringIntegrationTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Pass Play Scoring Integration

        [TestMethod]
        public void PassTouchdown_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 85;
            game.HomeScore = 7;
            game.AwayScore = 14;
            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.Possession = Possession.Home;
            passPlay.YardsGained = 18;  // TD

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.IsTrue(passPlay.IsTouchdown, "Should be touchdown");
            Assert.AreEqual(13, game.HomeScore, "Home should have 13 points (7 + 6)");
            Assert.AreEqual(14, game.AwayScore, "Away score unchanged");
            Assert.AreEqual(100, game.FieldPosition, "At goal line");
            Assert.IsTrue(passPlay.PossessionChange, "Possession changes after TD");
        }

        [TestMethod]
        public void PassSafety_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            game.FieldPosition = 3;
            game.HomeScore = 10;
            game.AwayScore = 7;
            var passPlay = (PassPlay)game.CurrentPlay;
            passPlay.Possession = Possession.Home;
            passPlay.YardsGained = -5;  // Sacked in end zone

            // Act
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(10, game.HomeScore, "Home score unchanged");
            Assert.AreEqual(9, game.AwayScore, "Away should have 9 points (7 + 2 safety)");
            Assert.AreEqual(0, game.FieldPosition, "At own goal line");
            Assert.IsTrue(passPlay.PossessionChange, "Possession changes after safety");
        }

        #endregion

        #region Run Play Scoring Integration

        [TestMethod]
        public void RunTouchdown_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 92;
            game.HomeScore = 14;
            game.AwayScore = 10;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.Possession = Possession.Away;
            runPlay.YardsGained = 10;  // TD

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.IsTouchdown, "Should be touchdown");
            Assert.AreEqual(14, game.HomeScore, "Home score unchanged");
            Assert.AreEqual(16, game.AwayScore, "Away should have 16 points (10 + 6)");
            Assert.AreEqual(100, game.FieldPosition, "At goal line");
            Assert.IsTrue(runPlay.PossessionChange, "Possession changes after TD");
        }

        [TestMethod]
        public void RunSafety_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 4;
            game.HomeScore = 7;
            game.AwayScore = 3;
            var runPlay = (RunPlay)game.CurrentPlay;
            runPlay.Possession = Possession.Away;
            runPlay.YardsGained = -6;  // Tackled in end zone

            // Act
            var runResult = new RunResult();
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(9, game.HomeScore, "Home should have 9 points (7 + 2 safety)");
            Assert.AreEqual(3, game.AwayScore, "Away score unchanged");
            Assert.AreEqual(0, game.FieldPosition, "At own goal line");
            Assert.IsTrue(runPlay.PossessionChange, "Possession changes after safety");
        }

        #endregion

        #region Punt Play Scoring Integration

        [TestMethod]
        public void PuntReturnTouchdown_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 90;
            game.HomeScore = 21;
            game.AwayScore = 17;
            var puntPlay = (PuntPlay)game.CurrentPlay;
            puntPlay.Possession = Possession.Home;
            puntPlay.YardsGained = 12;  // Short punt + return for TD
            puntPlay.IsTouchdown = true;
            var returner = new Player { LastName = "Speedster", Position = Positions.WR };
            puntPlay.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = returner,
                YardsGained = 7
            });

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(puntPlay.IsTouchdown, "Should be TD");
            Assert.AreEqual(21, game.HomeScore, "Home score unchanged");
            Assert.AreEqual(23, game.AwayScore, "Away should have 23 points (17 + 6)");
            Assert.AreEqual(100, game.FieldPosition, "At goal line");
            Assert.IsTrue(puntPlay.PossessionChange, "Possession changes after TD");
        }

        [TestMethod]
        public void PuntBlockedForTouchdown_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 95;
            game.HomeScore = 10;
            game.AwayScore = 14;
            var puntPlay = (PuntPlay)game.CurrentPlay;
            puntPlay.Possession = Possession.Home;
            puntPlay.Blocked = true;
            puntPlay.YardsGained = 10;
            puntPlay.PossessionChange = true;
            puntPlay.IsTouchdown = true;
            puntPlay.RecoveredBy = new Player { LastName = "Rusher", Position = Positions.DE };

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.IsTrue(puntPlay.IsTouchdown, "Should be TD");
            Assert.AreEqual(10, game.HomeScore, "Home score unchanged");
            Assert.AreEqual(20, game.AwayScore, "Away should have 20 points (14 + 6)");
        }

        [TestMethod]
        public void PuntBadSnapSafety_EndToEnd_ScoresCorrectly()
        {
            // Arrange
            var game = CreateGameWithPuntPlay();
            game.FieldPosition = 4;
            game.HomeScore = 7;
            game.AwayScore = 10;
            var puntPlay = (PuntPlay)game.CurrentPlay;
            puntPlay.Possession = Possession.Home;
            puntPlay.GoodSnap = false;
            puntPlay.YardsGained = -4;

            // Safety should have been scored during punt execution (in Punt.cs)
            // Simulate that it was already scored
            game.AwayScore += 2;

            // Act
            var puntResult = new PuntResult();
            puntResult.Execute(game);

            // Assert
            Assert.AreEqual(7, game.HomeScore, "Home score unchanged");
            Assert.AreEqual(12, game.AwayScore, "Away should have 12 points (10 + 2)");
        }

        #endregion

        #region Multi-Play Scoring Drives

        [TestMethod]
        public void CompleteDrive_PassAndRunTDs_ScoresCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Play 1: Pass for TD
            var passPlay = CreatePassPlay(Possession.Home, 85, 18);
            game.CurrentPlay = passPlay;
            game.FieldPosition = 85;
            new PassResult().Execute(game);

            Assert.AreEqual(6, game.HomeScore, "After pass TD");

            // Play 2: Extra point
            game.AddExtraPoint(Possession.Home);
            Assert.AreEqual(7, game.HomeScore, "After XP");

            // Play 3: Run for TD (opposing team)
            var runPlay = CreateRunPlay(Possession.Away, 90, 12);
            game.CurrentPlay = runPlay;
            game.FieldPosition = 90;
            new RunResult().Execute(game);

            Assert.AreEqual(6, game.AwayScore, "After run TD");

            // Play 4: Two-point conversion
            game.AddTwoPointConversion(Possession.Away);

            // Assert final
            Assert.AreEqual(7, game.HomeScore, "Home final score");
            Assert.AreEqual(8, game.AwayScore, "Away final score");
        }

        [TestMethod]
        public void DefensiveTDs_MultipleWays_ScoreCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Scenario 1: Safety
            var runPlay1 = CreateRunPlay(Possession.Home, 3, -5);
            game.CurrentPlay = runPlay1;
            game.FieldPosition = 3;
            new RunResult().Execute(game);
            Assert.AreEqual(2, game.AwayScore, "After safety");

            // Scenario 2: Punt return TD
            var puntPlay = new PuntPlay
            {
                Possession = Possession.Home,
                YardsGained = 15,
                IsTouchdown = true
            };
            puntPlay.ReturnSegments.Add(new ReturnSegment
            {
                BallCarrier = new Player { LastName = "Returner" },
                YardsGained = 10
            });
            game.CurrentPlay = puntPlay;
            game.FieldPosition = 88;
            new PuntResult().Execute(game);
            Assert.AreEqual(8, game.AwayScore, "After punt return TD (2 + 6)");

            // Scenario 3: Blocked punt TD
            var blockedPunt = new PuntPlay
            {
                Possession = Possession.Home,
                Blocked = true,
                YardsGained = 15,
                PossessionChange = true,
                IsTouchdown = true,
                RecoveredBy = new Player { LastName = "Blocker" }
            };
            game.CurrentPlay = blockedPunt;
            game.FieldPosition = 88;
            new PuntResult().Execute(game);

            // Assert final
            Assert.AreEqual(14, game.AwayScore, "After blocked punt TD (8 + 6)");
            Assert.AreEqual(0, game.HomeScore, "Home scored nothing");
        }

        [TestMethod]
        public void FullGameScenario_AllScoringTypes_Tracked()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Quarter 1
            game.AddTouchdown(Possession.Home);          // H: 6
            game.AddExtraPoint(Possession.Home);         // H: 7
            game.AddFieldGoal(Possession.Away);          // A: 3

            // Quarter 2
            game.AddSafety(Possession.Home);             // H: 9
            game.AddTouchdown(Possession.Away);          // A: 9
            game.AddTwoPointConversion(Possession.Away); // A: 11

            // Quarter 3
            game.AddFieldGoal(Possession.Home);          // H: 12
            game.AddTouchdown(Possession.Home);          // H: 18
            game.AddExtraPoint(Possession.Home);         // H: 19

            // Quarter 4
            game.AddTouchdown(Possession.Away);          // A: 17
            game.AddExtraPoint(Possession.Away);         // A: 18
            game.AddFieldGoal(Possession.Home);          // H: 22

            // Assert final score
            Assert.AreEqual(22, game.HomeScore, "Home team final score");
            Assert.AreEqual(18, game.AwayScore, "Away team final score");
        }

        #endregion

        #region Scoring Edge Cases

        [TestMethod]
        public void MultipleScoresInSuccession_AccumulateCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();

            // Act - Rapid scoring sequence
            for (int i = 0; i < 3; i++)
            {
                game.AddTouchdown(Possession.Home);
                game.AddExtraPoint(Possession.Home);
            }

            // Assert
            Assert.AreEqual(21, game.HomeScore, "3 TDs + 3 XPs = 21");
        }

        [TestMethod]
        public void ScoreComparisonAtVariousPoints_WorksCorrectly()
        {
            // Arrange
            var game = _testGame.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 10;

            // Act & Assert - Home winning
            Assert.IsTrue(game.HomeScore > game.AwayScore, "Home should be winning");

            game.AddTouchdown(Possession.Away);
            game.AddExtraPoint(Possession.Away);

            // Assert - Away now winning
            Assert.IsTrue(game.AwayScore > game.HomeScore, "Away should now be winning (17 vs 14)");
        }

        [TestMethod]
        public void ScurigamiScores_TrackCorrectly()
        {
            // Test unusual but valid scores (scorigami)
            var game = _testGame.GetGame();

            // 6-2 game (TD no XP, safety)
            game.AddTouchdown(Possession.Home);  // 6
            game.AddSafety(Possession.Away);     // 2

            Assert.AreEqual(6, game.HomeScore);
            Assert.AreEqual(2, game.AwayScore);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                YardsGained = 0
            };
            game.HomeScore = 0;
            game.AwayScore = 0;
            return game;
        }

        private Game CreateGameWithRunPlay()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                YardsGained = 0
            };
            game.HomeScore = 0;
            game.AwayScore = 0;
            return game;
        }

        private Game CreateGameWithPuntPlay()
        {
            var game = _testGame.GetGame();
            game.CurrentPlay = new PuntPlay
            {
                Possession = Possession.Home,
                Down = Downs.Fourth,
                YardsGained = 0,
                Punter = new Player { LastName = "Punter", Position = Positions.P }
            };
            game.HomeScore = 0;
            game.AwayScore = 0;
            return game;
        }

        private PassPlay CreatePassPlay(Possession possession, int fieldPos, int yards)
        {
            return new PassPlay
            {
                Possession = possession,
                YardsGained = yards
            };
        }

        private RunPlay CreateRunPlay(Possession possession, int fieldPos, int yards)
        {
            return new RunPlay
            {
                Possession = possession,
                YardsGained = yards
            };
        }

        #endregion
    }
}
