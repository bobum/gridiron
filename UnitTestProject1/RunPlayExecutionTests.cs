using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class RunPlayExecutionTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Basic Run Play Execution Tests

        [TestMethod]
        public void RunPlay_BasicExecution_CreatesRunSegment()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.6, [3] = 0.8, [4] = 6.2 }, // QB check, blocking, tackle break, big run, elapsed time
                __NextInt = { [0] = 4 } // Direction
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(1, runPlay.RunSegments.Count, "Should have exactly 1 run segment");
            Assert.IsNotNull(runPlay.RunSegments[0].BallCarrier, "Ball carrier should be assigned");
            Assert.IsTrue(runPlay.YardsGained != 0, "Yards gained should be calculated");
            Assert.IsTrue(runPlay.ElapsedTime > 0, "Elapsed time should be set");
        }

        [TestMethod]
        public void RunPlay_RBGetsBallMostOfTime_QBOccasionally()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;

            // Test RB gets the ball (NextDouble > 0.10)
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.11, [1] = 0.5, [2] = 0.6, [3] = 0.8, [4] = 6.0 },
                __NextInt = { [0] = 4 }
            };

            var run = new Run(rng);
            run.Execute(game);

            Assert.AreEqual(Positions.RB, runPlay.InitialBallCarrier.Position, "RB should get the ball when random > 0.10");
        }

        [TestMethod]
        public void RunPlay_QBScramble_WhenRandomUnder10Percent()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;

            // Test QB scramble (NextDouble < 0.10)
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.05, [1] = 0.5, [2] = 0.6, [3] = 0.8, [4] = 6.0 },
                __NextInt = { [0] = 4 }
            };

            var run = new Run(rng);
            run.Execute(game);

            Assert.AreEqual(Positions.QB, runPlay.InitialBallCarrier.Position, "QB should scramble when random < 0.10");
        }

        [TestMethod]
        public void RunPlay_DirectionIsSet_FromRandomSelection()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.6, [3] = 0.8, [4] = 6.0 },
                __NextInt = { [0] = 2 } // Middle direction
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.AreEqual(RunDirection.Middle, runPlay.RunSegments[0].Direction, "Direction should be set from random selection");
        }

        #endregion

        #region Blocking Impact Tests

        [TestMethod]
        public void RunPlay_GoodBlocking_IncreasesYardage()
        {
            // Arrange
            var game1 = CreateGameWithRunPlay();
            var game2 = CreateGameWithRunPlay();

            // Set consistent base yardage
            SetPlayerSkills(game1, offenseSkill: 70, defenseSkill: 70);
            SetPlayerSkills(game2, offenseSkill: 70, defenseSkill: 70);

            // Test with good blocking (blocking check succeeds)
            TestSeedableRandom rngGoodBlocking = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.4, [2] = 0.5, [3] = 0.6, [4] = 0.8, [5] = 6.0 }, // QB, blocking=SUCCESS, random factor, tackle, big run, time
                __NextInt = { [0] = 2 }
            };

            // Test with bad blocking (blocking check fails)
            TestSeedableRandom rngBadBlocking = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.6, [2] = 0.5, [3] = 0.6, [4] = 0.8, [5] = 6.0 }, // QB, blocking=FAIL, random factor, tackle, big run, time
                __NextInt = { [0] = 2 }
            };

            // Act
            var runGood = new Run(rngGoodBlocking);
            runGood.Execute(game1);

            var runBad = new Run(rngBadBlocking);
            runBad.Execute(game2);

            // Assert
            var yardsWithGoodBlocking = game1.CurrentPlay.YardsGained;
            var yardsWithBadBlocking = game2.CurrentPlay.YardsGained;

            Assert.IsTrue(yardsWithGoodBlocking > yardsWithBadBlocking,
                $"Good blocking ({yardsWithGoodBlocking}) should yield more yards than bad blocking ({yardsWithBadBlocking})");
        }

        #endregion

        #region Tackle Break Tests

        [TestMethod]
        public void RunPlay_TackleBreak_AddsExtraYards()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Ensure tackle break occurs
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.5, [3] = 0.1, [4] = 0.8, [5] = 6.0 }, // QB check, blocking, base yards random factor, tackle break=SUCCESS, big run, elapsed time
                __NextInt = { [0] = 2, [1] = 5 } // Direction, tackle break yards (5)
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.IsTrue(runPlay.YardsGained > 0, "Should have positive yards after tackle break");
        }

        #endregion

        #region Big Run Tests

        [TestMethod]
        public void RunPlay_BigRunBreakaway_AddsSignificantYards()
        {
            // Arrange
            var game1 = CreateGameWithRunPlay();
            var game2 = CreateGameWithRunPlay();
            SetPlayerSkills(game1, offenseSkill: 70, defenseSkill: 70);
            SetPlayerSkills(game2, offenseSkill: 70, defenseSkill: 70);

            // Set high speed for big run potential
            var ballCarrier1 = game1.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier1.Speed = 95;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2.Speed = 95;

            // Test WITHOUT big run
            TestSeedableRandom rngNoBigRun = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.5, [3] = 0.8, [4] = 0.9, [5] = 6.0 }, // Big run=FAIL
                __NextInt = { [0] = 2 }
            };

            // Test WITH big run
            TestSeedableRandom rngBigRun = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.5, [3] = 0.8, [4] = 0.05, [5] = 6.0 }, // Big run=SUCCESS
                __NextInt = { [0] = 2, [1] = 25 } // Direction, breakaway yards
            };

            // Act
            var runNormal = new Run(rngNoBigRun);
            runNormal.Execute(game1);

            var runBig = new Run(rngBigRun);
            runBig.Execute(game2);

            // Assert
            var normalYards = game1.CurrentPlay.YardsGained;
            var bigRunYards = game2.CurrentPlay.YardsGained;

            Assert.IsTrue(bigRunYards > normalYards + 10,
                $"Big run ({bigRunYards}) should add significantly more yards than normal run ({normalYards})");
        }

        #endregion

        #region Field Boundary Tests

        [TestMethod]
        public void RunPlay_RespectsTouchdownBoundary_DoesNotExceed100Yards()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 95; // Near the goal line
            SetPlayerSkills(game, offenseSkill: 90, defenseSkill: 50); // Set up for big gain

            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.4, [2] = 0.5, [3] = 0.05, [4] = 0.01, [5] = 6.0 }, // Good blocking, tackle break, big run
                __NextInt = { [0] = 2, [1] = 5, [2] = 30 } // Direction, tackle yards, big run yards
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            var yardsToGoal = 100 - 95; // Should be capped at 5 yards
            Assert.IsTrue(game.CurrentPlay.YardsGained <= yardsToGoal,
                $"Yards gained ({game.CurrentPlay.YardsGained}) should not exceed yards to goal ({yardsToGoal})");
        }

        [TestMethod]
        public void RunPlay_CanResultInNegativeYards_TackleForLoss()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 30, defenseSkill: 90); // Weak offense vs strong defense

            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.7, [2] = -2.5, [3] = 0.8, [4] = 0.9, [5] = 6.0 }, // Bad blocking, negative random factor
                __NextInt = { [0] = 2 }
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - can have negative yards (tackle for loss)
            Assert.IsTrue(game.CurrentPlay.YardsGained <= 5, "Should be able to lose yards or gain minimal yards");
        }

        #endregion

        #region Elapsed Time Tests

        [TestMethod]
        public void RunPlay_SetsElapsedTime_Between5And8Seconds()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var initialElapsedTime = game.CurrentPlay.ElapsedTime;
            TestSeedableRandom rng = new TestSeedableRandom
            {
                __NextDouble = { [0] = 0.15, [1] = 0.5, [2] = 0.5, [3] = 0.8, [4] = 0.9, [5] = 0.25 }, // Last value is elapsed time random (2.5)
                __NextInt = { [0] = 2 }
            };

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            var timeAdded = game.CurrentPlay.ElapsedTime - initialElapsedTime;
            Assert.IsTrue(timeAdded >= 5.0 && timeAdded <= 8.0,
                $"Elapsed time ({timeAdded}) should be between 5 and 8 seconds");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithRunPlay()
        {
            var game = _testGame.GetGame();

            // Create a run play with proper formations
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25,
                ElapsedTime = 0
            };

            // Set offensive players
            runPlay.OffensePlayersOnField = new List<Player>
            {
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.FB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.C][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            // Set defensive players
            runPlay.DefensePlayersOnField = new List<Player>
            {
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][2],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = runPlay;
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
                player.Rushing = offenseSkill;
                player.Speed = offenseSkill;
                player.Agility = offenseSkill;
                player.Strength = offenseSkill;
            }

            // Set all defensive players to the same skill level
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = defenseSkill;
                player.Strength = defenseSkill;
                player.Speed = defenseSkill;
            }
        }

        #endregion
    }
}
