using DomainObjects;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using System;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class TwoPointConversionTests
    {
        private TestGame? _testGame;

        [TestInitialize]
        public void Setup()
        {
            _testGame = new TestGame();
        }

        #region Successful Two-Point Conversion Tests

        [TestMethod]
        public void TwoPointConversion_SuccessfulRun_Adds2Points()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: true);
            var play = (RunPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 2; // Enough to score from the 2-yard line
            game.HomeScore = 6; // Just scored TD
            game.AwayScore = 0;

            var runResult = new RunResult();

            // Act
            runResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be marked as touchdown");
            Assert.IsTrue(play.IsTwoPointConversion, "Should be marked as two-point conversion");
            Assert.AreEqual(8, game.HomeScore, "Home score should be 8 (6 + 2)");
            Assert.AreEqual(0, game.AwayScore, "Away score should stay at 0");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void TwoPointConversion_SuccessfulPass_Adds2Points()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: false);
            var play = (PassPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            play.YardsGained = 2; // Enough to score from the 2-yard line
            game.HomeScore = 7;
            game.AwayScore = 6; // Just scored TD

            var passResult = new PassResult();

            // Act
            passResult.Execute(game);

            // Assert
            Assert.IsTrue(play.IsTouchdown, "Should be marked as touchdown");
            Assert.IsTrue(play.IsTwoPointConversion, "Should be marked as two-point conversion");
            Assert.AreEqual(7, game.HomeScore, "Home score should stay at 7");
            Assert.AreEqual(8, game.AwayScore, "Away score should be 8 (6 + 2)");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
        }

        [TestMethod]
        public void TwoPointConversion_SuccessfulRun_MultipleAttempts()
        {
            // Arrange
            var game = _testGame!.GetGame();
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Act - Simulate multiple successful 2pt conversions
            // Home scores TD + 2pt
            game.AddTouchdown(Possession.Home);      // Home: 6
            var play1 = CreateTwoPointRun(game, Possession.Home, yardsGained: 2);
            new RunResult().Execute(game);           // Home: 8

            // Away scores TD + 2pt
            game.AddTouchdown(Possession.Away);      // Away: 6
            var play2 = CreateTwoPointRun(game, Possession.Away, yardsGained: 3);
            new RunResult().Execute(game);           // Away: 8

            // Home scores another TD + 2pt
            game.AddTouchdown(Possession.Home);      // Home: 14
            var play3 = CreateTwoPointRun(game, Possession.Home, yardsGained: 2);
            new RunResult().Execute(game);           // Home: 16

            // Assert
            Assert.AreEqual(16, game.HomeScore, "Home should have 16 points");
            Assert.AreEqual(8, game.AwayScore, "Away should have 8 points");
        }

        #endregion

        #region Failed Two-Point Conversion Tests

        [TestMethod]
        public void TwoPointConversion_FailedRun_NoPoints()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: true);
            var play = (RunPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 1; // Stopped short of the goal line
            game.HomeScore = 6; // Just scored TD
            game.AwayScore = 0;

            var runResult = new RunResult();

            // Act
            runResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be touchdown");
            Assert.IsTrue(play.IsTwoPointConversion, "Should be marked as two-point conversion");
            Assert.AreEqual(6, game.HomeScore, "Home score should stay at 6");
            Assert.AreEqual(0, game.AwayScore, "Away score should stay at 0");
            Assert.IsTrue(play.PossessionChange, "Possession should change on failed 2pt");
            Assert.AreEqual(99, game.FieldPosition, "Field position should be 99 (98 + 1 yard)");
        }

        [TestMethod]
        public void TwoPointConversion_FailedPass_NoPoints()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: false);
            var play = (PassPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            play.YardsGained = 0; // Incomplete or no gain
            game.HomeScore = 14;
            game.AwayScore = 6; // Just scored TD

            var passResult = new PassResult();

            // Act
            passResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be touchdown");
            Assert.IsTrue(play.IsTwoPointConversion, "Should be marked as two-point conversion");
            Assert.AreEqual(14, game.HomeScore, "Home score should stay at 14");
            Assert.AreEqual(6, game.AwayScore, "Away score should stay at 6");
            Assert.IsTrue(play.PossessionChange, "Possession should change on failed 2pt");
        }

        [TestMethod]
        public void TwoPointConversion_StoppedAtGoalLine_NoPoints()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: true);
            var play = (RunPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 1; // Gets to 99-yard line, one yard short
            game.HomeScore = 6;
            game.AwayScore = 7;

            var runResult = new RunResult();

            // Act
            runResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be touchdown");
            Assert.AreEqual(6, game.HomeScore, "Home score should stay at 6");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(99, play.EndFieldPosition, "Should end at 99-yard line");
        }

        #endregion

        #region Loss of Yardage Tests

        [TestMethod]
        public void TwoPointConversion_LossOfYardage_NoPoints()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: true);
            var play = (RunPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = -3; // Sacked/tackled for loss
            game.HomeScore = 6;
            game.AwayScore = 0;

            var runResult = new RunResult();

            // Act
            runResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be touchdown");
            Assert.AreEqual(6, game.HomeScore, "Home score should stay at 6");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(95, game.FieldPosition, "Should be at 95-yard line (98 - 3)");
        }

        [TestMethod]
        public void TwoPointConversion_PassSacked_NoPoints()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: false);
            var play = (PassPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            play.YardsGained = -5; // Sacked
            game.HomeScore = 10;
            game.AwayScore = 6;

            var passResult = new PassResult();

            // Act
            passResult.Execute(game);

            // Assert
            Assert.IsFalse(play.IsTouchdown, "Should not be touchdown");
            Assert.AreEqual(6, game.AwayScore, "Away score should stay at 6");
            Assert.IsTrue(play.PossessionChange, "Possession should change");
            Assert.AreEqual(93, game.FieldPosition, "Should be at 93-yard line (98 - 5)");
        }

        #endregion

        #region Scoring Scenarios

        [TestMethod]
        public void TwoPointConversion_HomeTeam_CorrectScoring()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: true);
            var play = (RunPlay)game.CurrentPlay;
            play.Possession = Possession.Home;
            play.YardsGained = 2;
            game.HomeScore = 20;
            game.AwayScore = 21;

            var runResult = new RunResult();

            // Act
            runResult.Execute(game);

            // Assert
            Assert.AreEqual(22, game.HomeScore, "Home should have 22 points (20 + 2)");
            Assert.AreEqual(21, game.AwayScore, "Away score should not change");
        }

        [TestMethod]
        public void TwoPointConversion_AwayTeam_CorrectScoring()
        {
            // Arrange
            var game = CreateGameWithTwoPointAttempt(isRun: false);
            var play = (PassPlay)game.CurrentPlay;
            play.Possession = Possession.Away;
            play.YardsGained = 3; // More than enough
            game.HomeScore = 14;
            game.AwayScore = 12;

            var passResult = new PassResult();

            // Act
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(14, game.HomeScore, "Home score should not change");
            Assert.AreEqual(14, game.AwayScore, "Away should have 14 points (12 + 2)");
        }

        #endregion

        #region Realistic Game Scenarios

        [TestMethod]
        public void TwoPointConversion_GameSituation_TieGame()
        {
            // Arrange - Home team down by 8, scores TD and needs 2pt to tie
            var game = _testGame!.GetGame();
            game.HomeScore = 13;
            game.AwayScore = 21;

            // Act
            game.AddTouchdown(Possession.Home); // Home: 19
            var play = CreateTwoPointRun(game, Possession.Home, yardsGained: 2);
            new RunResult().Execute(game);      // Home: 21

            // Assert
            Assert.AreEqual(21, game.HomeScore, "Should tie the game at 21");
            Assert.AreEqual(21, game.AwayScore, "Should tie the game at 21");
        }

        [TestMethod]
        public void TwoPointConversion_GameSituation_TakeLead()
        {
            // Arrange - Away team down by 1, scores TD and 2pt to take lead
            var game = _testGame!.GetGame();
            game.HomeScore = 24;
            game.AwayScore = 18;

            // Act
            game.AddTouchdown(Possession.Away);  // Away: 24
            var play = CreateTwoPointPass(game, Possession.Away, yardsGained: 2);
            new PassResult().Execute(game);      // Away: 26

            // Assert
            Assert.AreEqual(24, game.HomeScore, "Home should have 24");
            Assert.AreEqual(26, game.AwayScore, "Away should take lead with 26");
        }

        [TestMethod]
        public void TwoPointConversion_Failed_StaysDown()
        {
            // Arrange - Home team down by 8, scores TD but fails 2pt
            var game = _testGame!.GetGame();
            game.HomeScore = 14;
            game.AwayScore = 22;

            // Act
            game.AddTouchdown(Possession.Home); // Home: 20
            var play = CreateTwoPointRun(game, Possession.Home, yardsGained: 1); // Stopped short
            new RunResult().Execute(game);      // Home: 20 (failed)

            // Assert
            Assert.AreEqual(20, game.HomeScore, "Should stay at 20 (failed 2pt)");
            Assert.AreEqual(22, game.AwayScore, "Away should still lead 22-20");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithTwoPointAttempt(bool isRun)
        {
            var game = _testGame!.GetGame();
            game.FieldPosition = 98; // 2-yard line (standard 2pt conversion spot)
            game.CurrentDown = Downs.First; // Doesn't really matter for 2pt
            game.YardsToGo = 2;

            if (isRun)
            {
                var runPlay = new RunPlay
                {
                    Possession = Possession.Home,
                    Down = Downs.First,
                    IsTwoPointConversion = true,
                    StartFieldPosition = 98,
                    OffensePlayersOnField = new List<Player>
                    {
                        new Player { Position = Positions.RB, LastName = "Runner", Speed = 70, Agility = 65 }
                    },
                    DefensePlayersOnField = new List<Player>()
                };

                runPlay.RunSegments.Add(new RunSegment
                {
                    BallCarrier = runPlay.OffensePlayersOnField[0],
                    YardsGained = 0 // Will be set in test
                });

                game.CurrentPlay = runPlay;
            }
            else
            {
                var passPlay = new PassPlay
                {
                    Possession = Possession.Home,
                    Down = Downs.First,
                    IsTwoPointConversion = true,
                    StartFieldPosition = 98,
                    OffensePlayersOnField = new List<Player>
                    {
                        new Player { Position = Positions.QB, LastName = "Quarterback", Passing = 75 },
                        new Player { Position = Positions.WR, LastName = "Receiver", Speed = 80 }
                    },
                    DefensePlayersOnField = new List<Player>()
                };

                passPlay.PassSegments.Add(new PassSegment
                {
                    Passer = passPlay.OffensePlayersOnField[0],
                    Receiver = passPlay.OffensePlayersOnField[1],
                    IsComplete = true,
                    AirYards = 2,
                    YardsAfterCatch = 0 // YardsGained will be computed
                });

                game.CurrentPlay = passPlay;
            }

            return game;
        }

        private RunPlay CreateTwoPointRun(Game game, Possession possession, int yardsGained)
        {
            game.FieldPosition = 98;
            var play = new RunPlay
            {
                Possession = possession,
                IsTwoPointConversion = true,
                YardsGained = yardsGained,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.RB, LastName = "Runner" }
                },
                DefensePlayersOnField = new List<Player>()
            };

            play.RunSegments.Add(new RunSegment
            {
                BallCarrier = play.OffensePlayersOnField[0],
                YardsGained = yardsGained
            });

            game.CurrentPlay = play;
            return play;
        }

        private PassPlay CreateTwoPointPass(Game game, Possession possession, int yardsGained)
        {
            game.FieldPosition = 98;
            var play = new PassPlay
            {
                Possession = possession,
                IsTwoPointConversion = true,
                YardsGained = yardsGained,
                OffensePlayersOnField = new List<Player>
                {
                    new Player { Position = Positions.QB, LastName = "Quarterback" },
                    new Player { Position = Positions.WR, LastName = "Receiver" }
                },
                DefensePlayersOnField = new List<Player>()
            };

            // PassSegment.YardsGained is computed from AirYards + YardsAfterCatch
            // For simplicity, split evenly or use all as air yards
            var airYards = Math.Min(yardsGained, 2);
            var yardsAfterCatch = Math.Max(0, yardsGained - airYards);

            play.PassSegments.Add(new PassSegment
            {
                Passer = play.OffensePlayersOnField[0],
                Receiver = play.OffensePlayersOnField[1],
                IsComplete = yardsGained >= 0,  // Complete if positive yards
                AirYards = airYards,
                YardsAfterCatch = yardsAfterCatch
            });

            game.CurrentPlay = play;
            return play;
        }

        #endregion
    }
}
