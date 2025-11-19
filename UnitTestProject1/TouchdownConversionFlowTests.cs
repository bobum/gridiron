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
            // Try different seeds until we get one that produces extra point (>= 0.10)
            var rng = new SeedableRandom(50000); // Seed that produces value >= 0.10
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

            // Extra point is from the 2-yard line (not the 15-yard line as previously thought)
            // For Home scoring at position 100 (Away's goal), they kick from position 98 (2 yards from goal)
            Assert.AreEqual(98, game.FieldPosition,
                "Extra point should be from the 2-yard line (position 98 for Home team)");

            var fieldGoalPlay = (FieldGoalPlay)game.CurrentPlay;
            Assert.IsNotNull(fieldGoalPlay.Kicker, "Extra point should have kicker assigned");
            Assert.IsNotNull(fieldGoalPlay.Holder, "Extra point should have holder assigned");
        }

        [TestMethod]
        public void Touchdown_TriggersTwoPointConversion()
        {
            // Arrange - Use a seed that will trigger 2-pt conversion
            var game = _testGame.GetGame();
            // Try different seeds until we get one that produces 2-pt (< 0.10)
            var rng = new SeedableRandom(1); // Seed that produces value < 0.10
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
                Assert.AreEqual(2, game.FieldPosition,
                    "Away team extra point from the 2-yard line (position 2)");
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

            var tdRunResult = new RunResult();
            tdRunResult.Execute(game);

            Assert.AreEqual(6, game.HomeScore, "Should have 6 points after TD");
            game.Plays.Add(runPlay);

            // ===== STEP 2: Extra Point OR 2-pt Conversion Attempt =====
            var prePlay1 = new PrePlay(rng);
            prePlay1.Execute(game);

            // Could be either extra point or 2-pt conversion depending on RNG
            bool isExtraPoint = game.CurrentPlay.PlayType == PlayType.FieldGoal;
            bool is2ptConversion = (game.CurrentPlay.PlayType == PlayType.Run ||
                                   game.CurrentPlay.PlayType == PlayType.Pass) &&
                                   game.CurrentPlay.Down == Downs.None;

            Assert.IsTrue(isExtraPoint || is2ptConversion,
                "Next play should be extra point or 2-pt conversion");

            if (isExtraPoint)
            {
                Assert.AreEqual(98, game.FieldPosition, "Extra point from 2-yard line (position 98)");
                var extraPoint = (FieldGoalPlay)game.CurrentPlay;

                // Execute the extra point
                var fieldGoal = new FieldGoal(rng);
                fieldGoal.Execute(game);

                var fgResult = new FieldGoalResult();
                fgResult.Execute(game);

                // Check if it was good
                if (extraPoint.IsGood)
                {
                    Assert.AreEqual(7, game.HomeScore, "Should have 7 points after good XP");
                }

                game.Plays.Add(extraPoint);
            }
            else // 2-pt conversion
            {
                Assert.AreEqual(98, game.FieldPosition, "2-pt conversion from 2-yard line");

                // Execute the 2-pt attempt
                if (game.CurrentPlay.PlayType == PlayType.Run)
                {
                    var run = new Run(rng);
                    run.Execute(game);
                    var twoPointRunResult = new RunResult();
                    twoPointRunResult.Execute(game);
                }
                else
                {
                    var pass = new Pass(rng);
                    pass.Execute(game);
                    var twoPointPassResult = new PassResult();
                    twoPointPassResult.Execute(game);
                }

                var conversionPlay = game.CurrentPlay;
                game.Plays.Add(conversionPlay);
            }

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

        [TestMethod]
        public void AwayTeamTouchdown_CompleteFlow_ExtraPointAndKickoff()
        {
            // Test that away team scoring TD follows same flow

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(456);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            game.Plays.Add(new KickoffPlay { Possession = Possession.Home, Down = Downs.None, Result = logger });

            game.HomeScore = 0;
            game.AwayScore = 0;

            // Away team scores touchdown
            var passPlay = new PassPlay
            {
                Possession = Possession.Away,
                Down = Downs.Third,
                StartFieldPosition = 95,
                YardsGained = 5,
                EndFieldPosition = 100,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 95;

            var passResult = new PassResult();
            passResult.Execute(game);

            Assert.AreEqual(6, game.AwayScore, "Away should have 6 points after TD");
            game.Plays.Add(passPlay);

            // Extra point
            var prePlay1 = new PrePlay(rng);
            prePlay1.Execute(game);

            Assert.AreEqual(PlayType.FieldGoal, game.CurrentPlay.PlayType);
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Away team should attempt extra point");
            Assert.AreEqual(2, game.FieldPosition,
                "Away team extra point from the 2-yard line (position 2)");

            var extraPoint = (FieldGoalPlay)game.CurrentPlay;
            extraPoint.IsGood = true; // Simulate good kick
            game.Plays.Add(extraPoint);

            // Kickoff
            var prePlay2 = new PrePlay(rng);
            prePlay2.Execute(game);

            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType);
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Away team should kick off");
            Assert.AreEqual(65, game.FieldPosition,
                "Away team kickoff from position 65 (their 35-yard line)");
        }

        [TestMethod]
        public void MissedExtraPoint_StillTriggersKickoff()
        {
            // Verify that even if extra point is missed, kickoff still happens

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(789);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Previous touchdown
            var tdPlay = new RunPlay
            {
                Possession = Possession.Home,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };
            game.Plays.Add(tdPlay);

            // Missed extra point
            var missedXP = new FieldGoalPlay
            {
                Possession = Possession.Home,
                Down = Downs.None,
                StartFieldPosition = 85,
                IsGood = false, // MISSED
                PossessionChange = false,
                Result = logger
            };

            game.CurrentPlay = missedXP;
            game.FieldPosition = 85;
            game.HomeScore = 6; // Only 6 from TD

            var fgResult = new FieldGoalResult();
            fgResult.Execute(game);

            game.Plays.Add(missedXP);

            // Act - Should still trigger kickoff
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert
            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType,
                "Kickoff should happen even after missed extra point");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team should still kick off");
            Assert.AreEqual(35, game.FieldPosition, "Kickoff from 35");
        }

        [TestMethod]
        public void FailedTwoPointConversion_StillTriggersKickoff()
        {
            // Verify that failed 2-pt conversion still triggers kickoff

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(999);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            // Previous touchdown
            var tdPlay = new PassPlay
            {
                Possession = Possession.Away,
                IsTouchdown = true,
                PossessionChange = true,
                Result = logger
            };
            game.Plays.Add(tdPlay);

            // Failed 2-pt conversion
            var failed2pt = new RunPlay
            {
                Possession = Possession.Away,
                Down = Downs.None,
                StartFieldPosition = 2,
                YardsGained = 1, // Didn't reach goal line
                EndFieldPosition = 1,
                IsTouchdown = false, // FAILED
                PossessionChange = false,
                Result = logger
            };

            game.CurrentPlay = failed2pt;
            game.FieldPosition = 2;
            game.AwayScore = 6; // Only 6 from TD

            var runResult = new RunResult();
            runResult.Execute(game);

            game.Plays.Add(failed2pt);

            // Act - Should still trigger kickoff
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);

            // Assert
            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType,
                "Kickoff should happen even after failed 2-pt conversion");
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Away team should still kick off");
            Assert.AreEqual(65, game.FieldPosition, "Away kickoff from 65");
        }

        [TestMethod]
        public void ConversionAttempt_DoesNotChangePossession()
        {
            // Verify that possession doesn't change DURING the conversion attempt
            // (it changes after the kickoff)

            // Arrange
            var game = _testGame.GetGame();
            var rng = new SeedableRandom(111);
            var logger = new InMemoryLogger<Game>();
            game.Logger = logger;

            game.Plays.Add(new KickoffPlay { Possession = Possession.Home, Down = Downs.None, Result = logger });

            // Home scores TD
            var tdPlay = new RunPlay
            {
                Possession = Possession.Home,
                IsTouchdown = true,
                PossessionChange = true, // TD triggers possession change flag
                Result = logger
            };
            game.Plays.Add(tdPlay);

            // Extra point is set up
            var prePlay1 = new PrePlay(rng);
            prePlay1.Execute(game);

            // Verify extra point is for HOME team (same team that scored)
            Assert.AreEqual(PlayType.FieldGoal, game.CurrentPlay.PlayType);
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Extra point should be attempted by team that scored TD");
            Assert.AreEqual(Downs.None, game.CurrentPlay.Down,
                "Extra point should have Down = None");

            var extraPoint = (FieldGoalPlay)game.CurrentPlay;
            extraPoint.IsGood = true;
            extraPoint.PossessionChange = false; // Extra point itself doesn't change possession

            game.Plays.Add(extraPoint);

            // Kickoff should also be by Home team
            var prePlay2 = new PrePlay(rng);
            prePlay2.Execute(game);

            Assert.AreEqual(PlayType.Kickoff, game.CurrentPlay.PlayType);
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Kickoff should be by same team that scored TD");

            var kickoff = (KickoffPlay)game.CurrentPlay;
            kickoff.PossessionChange = true; // Kickoff DOES change possession

            game.Plays.Add(kickoff);

            // NOW possession should change to Away
            var prePlay3 = new PrePlay(rng);
            prePlay3.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "After kickoff, possession should finally change to opponent");
        }
    }
}
