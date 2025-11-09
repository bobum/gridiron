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
                .NextDouble(0.1)                     // Base yards random factor (produces negative yards: 0.1 * 25 - 15 = -12.5)
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

        #region SkillsCheckResult Integration Tests

        [TestMethod]
        public void RunPlay_RunYardsSkillsCheckResult_CalculatesBaseYards()
        {
            // Arrange - Test that RunYardsSkillsCheckResult correctly calculates base yardage
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking fails (>= 0.5)
                .NextDouble(0.72)                    // Base yards random factor: 0.72*25-15 = 3.0
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have base yards calculated by RunYardsSkillsCheckResult
            // With even matchup (70 vs 70): skillDiff = 0, baseYards = 3.0, randomFactor = 3.0, total = 6
            // After blocking fails (0.8x): 6 * 0.8 = 4.8 ≈ 5 yards
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained >= 4 && yardsGained <= 8,
                $"RunYardsSkillsCheckResult should calculate yards in expected range (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_RunYardsSkillsCheckResult_StrongOffense()
        {
            // Arrange - Test RunYardsSkillsCheckResult with strong offense vs weak defense
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 90, defenseSkill: 50);

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.4)               // Good blocking (succeeds)
                .NextDouble(0.7)                     // Base yards random factor: 0.7*25-15 = 2.5
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have higher base yards with skill advantage
            // skillDiff ≈ +40, baseYards = 3.0 + 40/20 = 5.0, randomFactor = 2.5, total = 7.5 → Round(8) * 1.2 = 9
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained >= 8,
                $"RunYardsSkillsCheckResult should calculate higher yards for strong offense (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_RunYardsSkillsCheckResult_WeakOffense()
        {
            // Arrange - Test RunYardsSkillsCheckResult with weak offense vs strong defense
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 40, defenseSkill: 80);

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.7)               // Bad blocking (fails)
                .NextDouble(0.2)                     // Base yards random factor: 0.2*25-15 = -10
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have lower/negative yards with skill disadvantage
            // skillDiff ≈ -40, baseYards = 3.0 - 2.0 = 1.0, randomFactor = -0.8, total ≈ 0.2 * 0.8 = 0
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained <= 3,
                $"RunYardsSkillsCheckResult should calculate low yards for weak offense (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_TackleBreakYardsSkillsCheckResult_AddsYards()
        {
            // Arrange - Test that TackleBreakYardsSkillsCheckResult adds 3-8 yards
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Set ball carrier to have high tackle break chance
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier.Rushing = 90;
            ballCarrier.Strength = 88;
            ballCarrier.Agility = 85;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards factor
                .TackleBreakCheck(0.1)               // TACKLE BREAK! (succeeds)
                .NextInt(6)                          // TackleBreakYardsSkillsCheckResult: 6 yards
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have base yards + tackle break yards (6)
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained >= 8,
                $"TackleBreakYardsSkillsCheckResult should add 6 yards (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_TackleBreakYardsSkillsCheckResult_Range()
        {
            // Arrange - Test that TackleBreakYardsSkillsCheckResult returns 3-8 yards
            var game1 = CreateGameWithRunPlay();
            var game2 = CreateGameWithRunPlay();
            SetPlayerSkills(game1, offenseSkill: 70, defenseSkill: 70);
            SetPlayerSkills(game2, offenseSkill: 70, defenseSkill: 70);

            var ballCarrier1 = game1.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier1.Rushing = 90;
            ballCarrier1.Strength = 88;
            ballCarrier1.Agility = 85;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2.Rushing = 90;
            ballCarrier2.Strength = 88;
            ballCarrier2.Agility = 85;

            // Test minimum (3 yards)
            var rngMin = new TestFluentSeedableRandom()
                .NextDouble(0.15)
                .NextInt(2)
                .RunBlockingCheck(0.5)
                .NextDouble(0.5)
                .TackleBreakCheck(0.1)
                .NextInt(3)                          // Min: 3 yards
                .BreakawayCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);

            // Test maximum (8 yards)
            var rngMax = new TestFluentSeedableRandom()
                .NextDouble(0.15)
                .NextInt(2)
                .RunBlockingCheck(0.5)
                .NextDouble(0.5)
                .TackleBreakCheck(0.1)
                .NextInt(8)                          // Max: 8 yards
                .BreakawayCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var runMin = new Run(rngMin);
            runMin.Execute(game1);

            var runMax = new Run(rngMax);
            runMax.Execute(game2);

            // Assert
            var yardsMin = game1.CurrentPlay.YardsGained;
            var yardsMax = game2.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsMax > yardsMin,
                $"TackleBreakYardsSkillsCheckResult with 8 yards ({yardsMax}) should exceed 3 yards ({yardsMin})");
        }

        [TestMethod]
        public void RunPlay_BreakawayYardsSkillsCheckResult_AddsSignificantYards()
        {
            // Arrange - Test that BreakawayYardsSkillsCheckResult adds 15-44 yards
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Set ball carrier to be fast for breakaway potential
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier.Speed = 95;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Blocking
                .NextDouble(0.5)                     // Base yards factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.05)                // BREAKAWAY! (succeeds)
                .NextInt(30)                         // BreakawayYardsSkillsCheckResult: 30 yards
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have base yards + breakaway yards (30)
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained >= 30,
                $"BreakawayYardsSkillsCheckResult should add 30 yards (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_BreakawayYardsSkillsCheckResult_Range()
        {
            // Arrange - Test BreakawayYardsSkillsCheckResult range (15-44 yards)
            var game1 = CreateGameWithRunPlay();
            var game2 = CreateGameWithRunPlay();
            SetPlayerSkills(game1, offenseSkill: 70, defenseSkill: 70);
            SetPlayerSkills(game2, offenseSkill: 70, defenseSkill: 70);

            var ballCarrier1 = game1.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier1.Speed = 95;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2.Speed = 95;

            // Test minimum (15 yards)
            var rngMin = new TestFluentSeedableRandom()
                .NextDouble(0.15)
                .NextInt(2)
                .RunBlockingCheck(0.5)
                .NextDouble(0.5)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.05)
                .NextInt(15)                         // Min: 15 yards
                .ElapsedTimeRandomFactor(0.5);

            // Test maximum (44 yards)
            var rngMax = new TestFluentSeedableRandom()
                .NextDouble(0.15)
                .NextInt(2)
                .RunBlockingCheck(0.5)
                .NextDouble(0.5)
                .TackleBreakCheck(0.9)
                .BreakawayCheck(0.05)
                .NextInt(44)                         // Max: 44 yards
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var runMin = new Run(rngMin);
            runMin.Execute(game1);

            var runMax = new Run(rngMax);
            runMax.Execute(game2);

            // Assert
            var yardsMin = game1.CurrentPlay.YardsGained;
            var yardsMax = game2.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsMax > yardsMin + 20,
                $"BreakawayYardsSkillsCheckResult max ({yardsMax}) should significantly exceed min ({yardsMin})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_IntegrationTest()
        {
            // Arrange - Complete integration test combining all SkillsCheckResults
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 75, defenseSkill: 70);

            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier.Rushing = 85;
            ballCarrier.Speed = 90;
            ballCarrier.Agility = 88;
            ballCarrier.Strength = 80;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.4)               // Good blocking (succeeds)
                .NextDouble(0.6)                     // Base yards: RunYardsSkillsCheckResult
                .TackleBreakCheck(0.1)               // Tackle break (succeeds)
                .NextInt(5)                          // TackleBreakYardsSkillsCheckResult: 5 yards
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Verify all components work together
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.AreEqual(1, runPlay.RunSegments.Count, "Should have 1 run segment");
            Assert.IsNotNull(runPlay.RunSegments[0].BallCarrier, "Ball carrier should be set");
            Assert.IsTrue(runPlay.YardsGained >= 10,
                $"Should have base + tackle break yards (got {runPlay.YardsGained})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_MaximumYardage()
        {
            // Arrange - Test maximum yardage scenario with all bonuses
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 20; // Plenty of room to run
            SetPlayerSkills(game, offenseSkill: 90, defenseSkill: 50);

            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier.Rushing = 95;
            ballCarrier.Speed = 98;
            ballCarrier.Agility = 95;
            ballCarrier.Strength = 90;

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.3)               // Great blocking (succeeds)
                .NextDouble(0.9)                     // Max base yards: 0.9*25-15 = 7.5
                .TackleBreakCheck(0.05)              // Tackle break (succeeds)
                .NextInt(8)                          // TackleBreakYardsSkillsCheckResult: 8 yards
                .BreakawayCheck(0.02)                // Breakaway (succeeds)
                .NextInt(40)                         // BreakawayYardsSkillsCheckResult: 40 yards
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have massive yardage from all bonuses
            // Base ≈ 10, * 1.2 = 12, + 8 tackle break = 20, + 40 breakaway = 60+
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained >= 50,
                $"Maximum scenario should produce 50+ yards (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_NegativeYardage()
        {
            // Arrange - Test negative yardage scenario (tackle for loss)
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 30, defenseSkill: 95);

            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.8)               // Bad blocking (fails)
                .NextDouble(0.05)                    // Minimal base yards: 0.05*25-15 = -13.75
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - RunYardsSkillsCheckResult should allow negative yards
            // skillDiff ≈ -65, baseYards ≈ -0.25, randomFactor = -13.75, total ≈ -14 * 0.8 ≈ -11
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsTrue(yardsGained <= 0,
                $"Weak offense should result in tackle for loss (got {yardsGained})");
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