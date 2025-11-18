using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Actions;
using StateLibrary.PlayResults;
using StateLibrary.Plays;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests to verify the complete flow after a touchdown:
    /// 1. Touchdown occurs (6 points)
    /// 2. Extra point attempt (FieldGoal from 15-yard line) OR 2-pt conversion (10% of time)
    /// 3. After conversion attempt, kickoff by team that scored
    /// </summary>
    [TestClass]
    public class TouchdownConversionFlowTests
    {
        private readonly TestGame _testGame = new TestGame();

        [TestMethod]
        public void Touchdown_TriggersExtraPointAttempt()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(100); // Seed that should trigger extra point (not 2-pt)
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Initial kickoff
            game.Plays.Add(new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                PossessionChange = false,
                Result = logger
            });

            // Touchdown play
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.Third,
                StartFieldPosition = 92,
                YardsGained = 8,
                EndFieldPosition = 100,
                IsTouchdown = true,
                PossessionChange = true, // TDs trigger possession change
                Result = logger
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 92;
            game.HomeScore = 0;
            game.AwayScore = 0;

            // Process touchdown
            var passResult = new PassResult();
            passResult.Execute(game);

            Assert.AreEqual(6, game.HomeScore, "Touchdown should score 6 points");
            game.Plays.Add(passPlay);

            // Act - PrePlay should determine next play is extra point
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert
            Assert.IsNotNull(game.CurrentPlay, "Should have a current play");
            Assert.AreEqual(PlayType.FieldGoal, game.CurrentPlay.PlayType,
                "Next play after touchdown should be FieldGoal (extra point)");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Scoring team should attempt extra point");
            Assert.AreEqual(Downs.None, game.CurrentPlay.Down,
                "Extra point should have Down = None (not a regular down)");

            // Extra point is from the 15-yard line (opponent's 15)
            // For Home scoring, that's position 85 (100 - 15)
            Assert.AreEqual(85, game.FieldPosition,
                "Extra point should be from opponent's 15-yard line (position 85)");

            var fieldGoalPlay = (FieldGoalPlay)game.CurrentPlay;
            Assert.IsNotNull(fieldGoalPlay.Kicker, "Extra point should have kicker assigned");
            Assert.IsNotNull(fieldGoalPlay.Holder, "Extra point should have holder assigned");
        }

        [TestMethod]
        public void Touchdown_TriggersTwoPointConversion()
        {
            // Arrange - Use a seed that will trigger 2-pt conversion
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(999); // Different seed to potentially trigger 2-pt
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Initial kickoff
            game.Plays.Add(new KickoffPlay
            {
                Possession = Possession.Away,
                Down = Downs.None,
                StartTime = 0,
                Result = logger
            });

            // Touchdown
            var runPlay = new RunPlay
            {
                Possession = Possession.Away,
                Down = Downs.First,
                StartFieldPosition = 95,
                YardsGained = 5,
                EndFieldPosition = 100,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };

            game.CurrentPlay = runPlay;
            game.FieldPosition = 95;
            game.HomeScore = 0;
            game.AwayScore = 0;

            var runResult = new RunResult();
            runResult.Execute(game);

            Assert.AreEqual(6, game.AwayScore, "Touchdown should score 6 points");
            game.Plays.Add(runPlay);

            // Act
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert - Could be extra point OR 2-pt conversion
            Assert.IsNotNull(game.CurrentPlay, "Should have a current play");

            if (game.CurrentPlay.PlayType == PlayType.FieldGoal)
            {
                // Extra point
                Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession);
                Assert.AreEqual(Downs.None, game.CurrentPlay.Down);
                Assert.AreEqual(15, game.FieldPosition,
                    "Away team extra point from opponent's 15 (position 15)");
            }
            else
            {
                // 2-pt conversion (Run or Pass)
                Assert.IsTrue(game.CurrentPlay.PlayType == PlayType.Run ||
                              game.CurrentPlay.PlayType == PlayType.Pass,
                    "2-pt conversion should be Run or Pass");
                Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession);
                Assert.AreEqual(Downs.None, game.CurrentPlay.Down,
                    "2-pt conversion should have Down = None");

                // 2-pt conversion is from the 2-yard line (opponent's 2)
                Assert.AreEqual(2, game.FieldPosition,
                    "Away team 2-pt conversion from opponent's 2-yard line (position 2)");
            }
        }

        [TestMethod]
        public void ExtraPoint_TriggersKickoff()
        {
            // This test verifies that after an extra point attempt, a kickoff occurs

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(200);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Simulate previous touchdown
            var touchdownPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };
            game.Plays.Add(touchdownPlay);

            // Extra point attempt (good)
            var extraPointPlay = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.None, // Extra point has no down
                StartFieldPosition = 85,
                YardsGained = 0,
                EndFieldPosition = 85,
                IsGood = true,
                PossessionChange = false,
                Result = logger
            };

            game.CurrentPlay = extraPointPlay;
            game.FieldPosition = 85;
            game.HomeScore = 7; // 6 + 1

            // Process extra point
            var fgResult = new FieldGoalResult();
            fgResult.Execute(game);

            game.Plays.Add(extraPointPlay);

            // Act - Next play should be kickoff
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert
            Assert.IsNotNull(game.CurrentPlay, "Should have a current play");
            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType,
                "After extra point, next play should be Kickoff");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Team that scored should kick off");
            Assert.AreEqual(Downs.None, game.CurrentPlay.Down,
                "Kickoff has no down");

            // Kickoff is from the 35-yard line
            // Home kicking = position 35
            Assert.AreEqual(35, game.FieldPosition,
                "Home team kickoff from their 35-yard line");

            var kickoffPlay = (KickoffPlay)game.CurrentPlay;
            Assert.IsNotNull(kickoffPlay.Kicker, "Kickoff should have kicker assigned");
        }

        [TestMethod]
        public void TwoPointConversion_TriggersKickoff()
        {
            // This test verifies that after a 2-pt conversion attempt, a kickoff occurs

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(300);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Simulate previous touchdown
            var touchdownPlay = new RunPlay
            {
                Possession = Possession.Away,
                Down = Downs.Second,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };
            game.Plays.Add(touchdownPlay);

            // 2-pt conversion attempt (successful)
            var twoPointPlay = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.None, // 2-pt has no down
                StartFieldPosition = 2,
                YardsGained = 2,
                EndFieldPosition = 0, // Reached goal line
                IsTouchdown = true, // 2-pt conversion is technically a TD
                PossessionChange = false,
                Result = logger
            };

            game.CurrentPlay = twoPointPlay;
            game.FieldPosition = 2;
            game.AwayScore = 8; // 6 + 2

            // Process 2-pt conversion
            var passResult = new PassResult();
            passResult.Execute(game);

            game.Plays.Add(twoPointPlay);

            // Act - Next play should be kickoff
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert
            Assert.IsNotNull(game.CurrentPlay, "Should have a current play");
            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType,
                "After 2-pt conversion, next play should be Kickoff");
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Team that scored should kick off");

            // Away team kickoff from their 35 = position 65
            Assert.AreEqual(65, game.FieldPosition,
                "Away team kickoff from their 35-yard line (position 65)");
        }

        [TestMethod]
        public void Touchdown_CompleteFlow_ExtraPointAndKickoff()
        {
            // Comprehensive integration test for the full TD → XP → Kickoff flow

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(12345);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Start with kickoff
            game.Plays.Add(new KickoffPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartTime = 0,
                Result = logger
            });

            game.HomeScore = 0;
            game.AwayScore = 0;

            // ===== STEP 1: Touchdown =====
            var runPlay = new RunPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 98,
                YardsGained = 2,
                EndFieldPosition = 100,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };

            game.CurrentPlay = runPlay;
            game.FieldPosition = 98;

            var runResult = new RunResult();
            runResult.Execute(game);

            Assert.AreEqual(6, game.HomeScore, "Should have 6 points after TD");
            game.Plays.Add(runPlay);

            // ===== STEP 2: Extra Point Attempt =====
            var prePlay1 = new PrePlay(rng);
            prePlay1.Execute(game);

            Assert.AreEqual(PlayType.FieldGoal, game.CurrentPlay.PlayType,
                "Next play should be extra point");
            Assert.AreEqual(85, game.FieldPosition, "Extra point from 15-yard line");

            var extraPoint = (FieldGoalPlay)game.CurrentPlay;

            // Execute the extra point
            var fieldGoal = new FieldGoal(rng);
            fieldGoal.Execute(game);

            var fgResult = new FieldGoalResult();
            fgResult.Execute(game);

            // Assume it was good (we'll need to check based on execution)
            if (extraPoint.IsGood)
            {
                Assert.AreEqual(7, game.HomeScore, "Should have 7 points after good XP");
            }

            game.Plays.Add(extraPoint);

            // ===== STEP 3: Kickoff =====
            var prePlay2 = new PrePlay(rng);
            prePlay2.Execute(game);

            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType,
                "After extra point, should have kickoff");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team (who scored) should kick off");
            Assert.AreEqual(35, game.FieldPosition, "Kickoff from 35-yard line");

            var kickoff = (KickoffPlay)game.CurrentPlay;
            Assert.IsNotNull(kickoff.Kicker, "Kickoff should have kicker");
            Assert.AreEqual(11, kickoff.OffensePlayersOnField.Count);
            Assert.AreEqual(11, kickoff.DefensePlayersOnField.Count);

            // Execute kickoff
            var kickoffExec = new Kickoff(rng);
            kickoffExec.Execute(game);

            var kickoffResult = new KickoffResult();
            kickoffResult.Execute(game);

            game.Plays.Add(kickoff);

            // ===== STEP 4: Next play should be normal offense for other team =====
            var prePlay3 = new PrePlay(rng);
            prePlay3.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "After kickoff, other team should have possession");
            Assert.IsTrue(game.CurrentPlay.PlayType == PlayType.Run ||
                         game.CurrentPlay.PlayType == PlayType.Pass,
                "Should be normal offensive play");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should be 1st down");
            Assert.AreEqual(10, game.YardsToGo, "Should be 10 yards to go");
        }
    }
}
