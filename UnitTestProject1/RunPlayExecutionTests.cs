using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (0.15 > 0.10 = RB gets ball)
                .NextInt(4)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking check (moderate success)
                .NextDouble(0.6)                     // Base yards random factor
                .NextDouble(0.8)                     // Tackle break check (fails)
                .NextDouble(0.9)                     // Big run check (fails)
                .ElapsedTimeRandomFactor(0.73);      // Elapsed time (5 + 0.73*3 = ~7.2 seconds)

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.11)                    // QB check (0.11 > 0.10 = RB gets ball)
                .NextInt(4)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.6)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time (5 + 0.33*3 = 6.0 seconds)

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.05)                    // QB check (0.05 < 0.10 = QB scrambles)
                .NextInt(4)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.6)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction = Middle
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.6)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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
            var rngGoodBlocking = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.4)               // Good blocking (succeeds)
                .NextDouble(0.5)                     // Base yards random factor
                .NextDouble(0.6)                     // Tackle break (fails)
                .NextDouble(0.8)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

            // Test with bad blocking (blocking check fails)
            var rngBadBlocking = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.6)               // Bad blocking (fails)
                .NextDouble(0.5)                     // Base yards random factor
                .NextDouble(0.6)                     // Tackle break (fails)
                .NextDouble(0.8)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards random factor
                .NextDouble(0.1)                     // Tackle break (succeeds!)
                .NextInt(5)                          // Tackle break yards (5)
                .NextDouble(0.8)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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
            var rngNoBigRun = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

            // Test WITH big run
            var rngBigRun = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.05)                    // Big run (succeeds!)
                .NextInt(25)                         // Breakaway yards
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.4)               // Good blocking
                .NextDouble(0.5)                     // Base yards factor
                .NextDouble(0.05)                    // Tackle break (succeeds)
                .NextInt(5)                          // Tackle break yards
                .NextDouble(0.01)                    // Big run (succeeds)
                .NextInt(30)                         // Breakaway yards (would exceed goal line)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.7)               // Bad blocking (fails)
                .NextDouble(0.1)                     // Base yards random factor (produces negative yards: 0.1 * 11 - 3 = -1.9)
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.33);      // Elapsed time

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
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards factor
                .NextDouble(0.8)                     // Tackle break (fails)
                .NextDouble(0.9)                     // Big run (fails)
                .ElapsedTimeRandomFactor(0.83);      // Elapsed time (5 + 0.83*3 = 7.5 seconds)

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