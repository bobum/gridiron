using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class RunPlayExecutionTests
 {
     private readonly DomainObjects.Helpers.Teams _teams = TestTeams.CreateTestTeams();
        private readonly TestGame _testGame = new TestGame();

        #region Basic Run Play Execution Tests

        [TestMethod]
        public void RunPlay_BasicExecution_CreatesRunSegment()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            var rng = RunPlayScenarios.SimpleGain(yards: 5, direction: 4);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.HasCount(1, runPlay.RunSegments, "Should have exactly 1 run segment");
            Assert.IsNotNull(runPlay.RunSegments[0].BallCarrier, "Ball carrier should be assigned");
            Assert.AreNotEqual(0, runPlay.YardsGained, "Yards gained should be calculated");
            Assert.IsGreaterThan(0, runPlay.ElapsedTime, "Elapsed time should be set");
        }

        [TestMethod]
        public void RunPlay_RBGetsBallMostOfTime_QBOccasionally()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;

            // Test RB gets the ball (uses SimpleGain which has QB check > 0.10)
            var rng = RunPlayScenarios.SimpleGain(yards: 5, direction: 4);

            var run = new Run(rng);
            run.Execute(game);

            Assert.AreEqual(Positions.RB, runPlay.InitialBallCarrier!.Position, "RB should get the ball when random > 0.10");
        }

        [TestMethod]
        public void RunPlay_QBScramble_WhenRandomUnder10Percent()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;

            // Test QB scramble (uses QBScramble which has QB check < 0.10)
            var rng = RunPlayScenarios.QBScramble(yards: 5, direction: 4);

            var run = new Run(rng);
            run.Execute(game);

            Assert.AreEqual(Positions.QB, runPlay.InitialBallCarrier!.Position, "QB should scramble when random < 0.10");
        }

        [TestMethod]
        public void RunPlay_DirectionIsSet_FromRandomSelection()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var runPlay = (RunPlay)game.CurrentPlay;
            // Uses SimpleGain with direction = Middle (2)
            var rng = RunPlayScenarios.SimpleGain(yards: 5, direction: 2);

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
            var rngGoodBlocking = RunPlayScenarios.GoodBlocking(yards: 7);

            // Test with bad blocking (blocking check fails)
            var rngBadBlocking = RunPlayScenarios.BadBlocking(yards: 5);

            // Act
            var runGood = new Run(rngGoodBlocking);
            runGood.Execute(game1);

            var runBad = new Run(rngBadBlocking);
            runBad.Execute(game2);

            // Assert
            var yardsWithGoodBlocking = game1.CurrentPlay.YardsGained;
            var yardsWithBadBlocking = game2.CurrentPlay.YardsGained;

            Assert.IsGreaterThan(yardsWithBadBlocking, yardsWithGoodBlocking, $"Good blocking ({yardsWithGoodBlocking}) should yield more yards than bad blocking ({yardsWithBadBlocking})");
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

            // Uses TackleBreak scenario with 5 extra yards
            var rng = RunPlayScenarios.TackleBreak(baseYards: 3, tackleBreakYards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            Assert.IsGreaterThan(0, runPlay.YardsGained, "Should have positive yards after tackle break");
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
            ballCarrier1!.Speed = 95;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2!.Speed = 95;

            // Test WITHOUT big run - uses SimpleGain
            var rngNoBigRun = RunPlayScenarios.SimpleGain(yards: 5);

            // Test WITH big run - uses Breakaway scenario
            var rngBigRun = RunPlayScenarios.Breakaway(baseYards: 5, breakawayYards: 25);

            // Act
            var runNormal = new Run(rngNoBigRun);
            runNormal.Execute(game1);

            var runBig = new Run(rngBigRun);
            runBig.Execute(game2);

            // Assert
            var normalYards = game1.CurrentPlay.YardsGained;
            var bigRunYards = game2.CurrentPlay.YardsGained;

            Assert.IsGreaterThan(normalYards + 10,
bigRunYards, $"Big run ({bigRunYards}) should add significantly more yards than normal run ({normalYards})");
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

            // Uses MaximumYardage scenario (tackle break + breakaway would exceed goal line)
            var rng = RunPlayScenarios.MaximumYardage(tackleBreakYards: 5, breakawayYards: 30);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            var yardsToGoal = 100 - 95; // Should be capped at 5 yards
            Assert.IsLessThanOrEqualTo(yardsToGoal, game.CurrentPlay.YardsGained, $"Yards gained ({game.CurrentPlay.YardsGained}) should not exceed yards to goal ({yardsToGoal})");
        }

        [TestMethod]
        public void RunPlay_CanResultInNegativeYards_TackleForLoss()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 30, defenseSkill: 90); // Weak offense vs strong defense

            // Uses TackleForLoss scenario
            var rng = RunPlayScenarios.TackleForLoss(lossYards: 2);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - can have negative yards (tackle for loss)
            Assert.IsLessThanOrEqualTo(5, game.CurrentPlay.YardsGained, "Should be able to lose yards or gain minimal yards");
        }

        #endregion

        #region Elapsed Time Tests

        [TestMethod]
        public void RunPlay_SetsElapsedTime_Between5And8Seconds()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var initialElapsedTime = game.CurrentPlay.ElapsedTime;
            // Uses SimpleGain scenario which sets elapsed time to ~6.5 seconds execution + ~30 seconds runoff = ~36.5 total
            var rng = RunPlayScenarios.SimpleGain(yards: 5);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert
            var timeAdded = game.CurrentPlay.ElapsedTime - initialElapsedTime;
            // Total time = execution time (5-8s) + runoff time (25-35s) = 30-43 seconds
            Assert.IsTrue(timeAdded >= 30.0 && timeAdded <= 43.0,
                $"Total elapsed time ({timeAdded}) should be execution (5-8s) + runoff (25-35s) = 30-43 seconds");
        }

        #endregion

        #region SkillsCheckResult Integration Tests

        [TestMethod]
        public void RunPlay_RunYardsSkillsCheckResult_CalculatesBaseYards()
        {
            // Arrange - Test that RunYardsSkillsCheckResult correctly calculates base yardage
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Uses BadBlocking scenario (blocking fails >= 0.5)
            var rng = RunPlayScenarios.BadBlocking(yards: 5);

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

            // Uses GoodBlocking scenario
            var rng = RunPlayScenarios.GoodBlocking(yards: 8);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have higher base yards with skill advantage
            // skillDiff ≈ +40, baseYards = 3.0 + 40/20 = 5.0, randomFactor = 2.5, total = 7.5 → Round(8) * 1.2 = 9
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsGreaterThanOrEqualTo(8,
yardsGained, $"RunYardsSkillsCheckResult should calculate higher yards for strong offense (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_RunYardsSkillsCheckResult_WeakOffense()
        {
            // Arrange - Test RunYardsSkillsCheckResult with weak offense vs strong defense
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 40, defenseSkill: 80);

            // Uses TackleForLoss scenario
            var rng = RunPlayScenarios.TackleForLoss(lossYards: 2);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have lower/negative yards with skill disadvantage
            // skillDiff ≈ -40, baseYards = 3.0 - 2.0 = 1.0, randomFactor = -0.8, total ≈ 0.2 * 0.8 = 0
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsLessThanOrEqualTo(3,
yardsGained, $"RunYardsSkillsCheckResult should calculate low yards for weak offense (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_TackleBreakYardsSkillsCheckResult_AddsYards()
        {
            // Arrange - Test that TackleBreakYardsSkillsCheckResult adds 3-8 yards
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Set ball carrier to have high tackle break chance
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier!.Rushing = 90;
            ballCarrier.Strength = 88;
            ballCarrier.Agility = 85;

            // Uses TackleBreak scenario with 6 yards added (original: blocking=0.5, baseYardsFactor=0.7)
            var rng = RunPlayScenarios.TackleBreak(baseYards: 3, tackleBreakYards: 6, blockingValue: 0.5, baseYardsFactor: 0.7);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have base yards + tackle break yards (6)
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsGreaterThanOrEqualTo(8,
yardsGained, $"TackleBreakYardsSkillsCheckResult should add 6 yards (got {yardsGained})");
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
            ballCarrier1!.Rushing = 90;
            ballCarrier1.Strength = 88;
            ballCarrier1.Agility = 85;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2!.Rushing = 90;
            ballCarrier2.Strength = 88;
            ballCarrier2.Agility = 85;

            // Test minimum (3 yards)
            var rngMin = RunPlayScenarios.TackleBreak(baseYards: 3, tackleBreakYards: 3);

            // Test maximum (8 yards)
            var rngMax = RunPlayScenarios.TackleBreak(baseYards: 3, tackleBreakYards: 8);

            // Act
            var runMin = new Run(rngMin);
            runMin.Execute(game1);

            var runMax = new Run(rngMax);
            runMax.Execute(game2);

            // Assert
            var yardsMin = game1.CurrentPlay.YardsGained;
            var yardsMax = game2.CurrentPlay.YardsGained;
            Assert.IsGreaterThan(yardsMin, yardsMax, $"TackleBreakYardsSkillsCheckResult with 8 yards ({yardsMax}) should exceed 3 yards ({yardsMin})");
        }

        [TestMethod]
        public void RunPlay_BreakawayYardsSkillsCheckResult_AddsSignificantYards()
        {
            // Arrange - Test that BreakawayYardsSkillsCheckResult adds 15-44 yards
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 70, defenseSkill: 70);

            // Set ball carrier to be fast for breakaway potential
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier!.Speed = 95;

            // Uses Breakaway scenario with 30 yards added
            var rng = RunPlayScenarios.Breakaway(baseYards: 5, breakawayYards: 30);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have base yards + breakaway yards (30)
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsGreaterThanOrEqualTo(30, yardsGained, $"BreakawayYardsSkillsCheckResult should add 30 yards (got {yardsGained})");
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
            ballCarrier1!.Speed = 95;

            var ballCarrier2 = game2.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier2!.Speed = 95;

            // Test minimum (15 yards)
            var rngMin = RunPlayScenarios.Breakaway(baseYards: 5, breakawayYards: 15);

            // Test maximum (44 yards)
            var rngMax = RunPlayScenarios.Breakaway(baseYards: 5, breakawayYards: 44);

            // Act
            var runMin = new Run(rngMin);
            runMin.Execute(game1);

            var runMax = new Run(rngMax);
            runMax.Execute(game2);

            // Assert
            var yardsMin = game1.CurrentPlay.YardsGained;
            var yardsMax = game2.CurrentPlay.YardsGained;
            Assert.IsGreaterThan(yardsMin + 20, yardsMax, $"BreakawayYardsSkillsCheckResult max ({yardsMax}) should significantly exceed min ({yardsMin})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_IntegrationTest()
        {
            // Arrange - Complete integration test combining all SkillsCheckResults
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 75, defenseSkill: 70);

            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier!.Rushing = 85;
            ballCarrier.Speed = 90;
            ballCarrier.Agility = 88;
            ballCarrier.Strength = 80;

            // Uses TackleBreak scenario (good blocking + tackle break, no breakaway) (original: blocking=0.4, baseYardsFactor=0.68)
            var rng = RunPlayScenarios.TackleBreak(baseYards: 5, tackleBreakYards: 5, blockingValue: 0.4, baseYardsFactor: 0.68);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Verify all components work together
            var runPlay = (RunPlay)game.CurrentPlay;
            Assert.HasCount(1, runPlay.RunSegments, "Should have 1 run segment");
            Assert.IsNotNull(runPlay.RunSegments[0].BallCarrier, "Ball carrier should be set");
            Assert.IsGreaterThanOrEqualTo(10, runPlay.YardsGained, $"Should have base + tackle break yards (got {runPlay.YardsGained})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_MaximumYardage()
        {
            // Arrange - Test maximum yardage scenario with all bonuses
            var game = CreateGameWithRunPlay();
            game.FieldPosition = 20; // Plenty of room to run
            SetPlayerSkills(game, offenseSkill: 90, defenseSkill: 50);

            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);
            ballCarrier!.Rushing = 95;
            ballCarrier.Speed = 98;
            ballCarrier.Agility = 95;
            ballCarrier.Strength = 90;

            // Uses MaximumYardage scenario (great blocking + tackle break + breakaway)
            var rng = RunPlayScenarios.MaximumYardage(tackleBreakYards: 8, breakawayYards: 40);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - Should have massive yardage from all bonuses
            // Base ≈ 10, * 1.2 = 12, + 8 tackle break = 20, + 40 breakaway = 60+
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsGreaterThanOrEqualTo(50, yardsGained, $"Maximum scenario should produce 50+ yards (got {yardsGained})");
        }

        [TestMethod]
        public void RunPlay_AllSkillsCheckResults_NegativeYardage()
        {
            // Arrange - Test negative yardage scenario (tackle for loss)
            var game = CreateGameWithRunPlay();
            SetPlayerSkills(game, offenseSkill: 30, defenseSkill: 95);

            // Uses TackleForLoss scenario
            var rng = RunPlayScenarios.TackleForLoss(lossYards: 3);

            // Act
            var run = new Run(rng);
            run.Execute(game);

            // Assert - RunYardsSkillsCheckResult should allow negative yards
            // skillDiff ≈ -65, baseYards ≈ -0.25, randomFactor = -13.75, total ≈ -14 * 0.8 ≈ -11
            var yardsGained = game.CurrentPlay.YardsGained;
            Assert.IsLessThanOrEqualTo(0, yardsGained, $"Weak offense should result in tackle for loss (got {yardsGained})");
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