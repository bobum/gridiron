using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Actions;
using StateLibrary.Plays;
using StateLibrary.PlayResults;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class DownProgressionTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region Down Progression Tests

        [TestMethod]
        public void DownProgression_FirstToSecond_WhenNotEnoughYards()
        {
            // Arrange - 1st and 10, gain 3 yards
            var game = CreateGameAtDown(Downs.First, 10, 25, Possession.Home);
            SetPlayerSkills(game, 70, 70); // Even matchup
            var rng = CreateRngForRunPlay(3, blockingSucceeds: true); // 3 yards gained

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(Downs.Second, game.CurrentDown, "Should advance to 2nd down");
            Assert.AreEqual(7, game.YardsToGo, "Should need 7 more yards (10 - 3)");
            Assert.AreEqual(28, game.FieldPosition, "Field position should be 25 + 3");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession, "Possession should not change");
        }

        [TestMethod]
        public void DownProgression_SecondToThird_WhenNotEnoughYards()
        {
            // Arrange - 2nd and 7, gain 4 yards
            var game = CreateGameAtDown(Downs.Second, 7, 28, Possession.Home);
            SetPlayerSkills(game, 70, 70); // Even matchup
            var rng = CreateRngForRunPlay(4, blockingSucceeds: true); // 4 yards gained

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Should advance to 3rd down");
            Assert.AreEqual(3, game.YardsToGo, "Should need 3 more yards (7 - 4)");
            Assert.AreEqual(32, game.FieldPosition, "Field position should be 28 + 4");
        }

        [TestMethod]
        public void DownProgression_ThirdToFourth_WhenNotEnoughYards()
        {
            // Arrange - 3rd and 3, gain 2 yards
            var game = CreateGameAtDown(Downs.Third, 3, 32, Possession.Home);
            SetPlayerSkills(game, 70, 70); // Even matchup
            var rng = CreateRngForRunPlay(2, blockingSucceeds: true); // 2 yards gained

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(Downs.Fourth, game.CurrentDown, "Should advance to 4th down");
            Assert.AreEqual(1, game.YardsToGo, "Should need 1 more yard (3 - 2)");
            Assert.AreEqual(34, game.FieldPosition, "Field position should be 32 + 2");
        }

        #endregion

        #region First Down Conversion Tests

        [TestMethod]
        public void FirstDownConversion_ResetsToFirstAnd10()
        {
            // Arrange - 3rd and 5, gain 12 yards (converts!)
            var game = CreateGameAtDown(Downs.Third, 5, 40, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(12, blockingSucceeds: true); // 12 yards - more than enough!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
            Assert.AreEqual(52, game.FieldPosition, "Field position should be 40 + 12");
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession, "Possession should not change");
        }

        [TestMethod]
        public void FirstDownConversion_ExactYardage_ResetsToFirstAnd10()
        {
            // Arrange - 2nd and 7, gain exactly 7 yards
            var game = CreateGameAtDown(Downs.Second, 7, 30, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(7, blockingSucceeds: true); // Exactly 7 yards

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down with exact yardage");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");
            Assert.AreEqual(37, game.FieldPosition, "Field position should be 30 + 7");
        }

        #endregion

        #region Turnover on Downs Tests

        [TestMethod]
        public void TurnoverOnDowns_FourthDownFailure_ChangesPossession()
        {
            // Arrange - Home has 4th and 5, gains only 2 yards (fails to convert)
            var game = CreateGameAtDown(Downs.Fourth, 5, 45, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(2, blockingSucceeds: true); // Only 2 yards - not enough!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Play result should set turnover
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "PossessionChange should be true");
            Assert.AreEqual(47, game.FieldPosition, "Field position should be 45 + 2");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down for new possession");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");

            // Add to game.Plays so PrePlay can check previous play
            game.Plays.Add(game.CurrentPlay);

            // Now simulate the next play to verify possession actually flips
            var nextPlayRng = new TestFluentSeedableRandom()
                .NextDouble(0.3); // Determines run vs pass

            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            // Assert - New play should have flipped possession
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession,
                "Next play should belong to Away team after turnover");
            Assert.AreEqual(Downs.First, game.CurrentPlay.Down, "Next play should be 1st down");
        }

        [TestMethod]
        public void TurnoverOnDowns_AwayTeamTurnsOver_GivesHomeFirstDown()
        {
            // Arrange - Away has 4th and 3, gains 1 yard (fails)
            var game = CreateGameAtDown(Downs.Fourth, 3, 60, Possession.Away);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(1, blockingSucceeds: true); // Only 1 yard

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "PossessionChange should be true");
            Assert.AreEqual(61, game.FieldPosition, "Field position should be 60 + 1");

            // Add to game.Plays so PrePlay can check previous play
            game.Plays.Add(game.CurrentPlay);

            // Simulate next play
            var nextPlayRng = new TestFluentSeedableRandom().NextDouble(0.3);
            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            // Assert - Home should get the ball
            Assert.AreEqual(Possession.Home, game.CurrentPlay.Possession,
                "Home team should get possession after Away turnover");
            Assert.AreEqual(Downs.First, game.CurrentPlay.Down, "Should be 1st down");
        }

        [TestMethod]
        public void TurnoverOnDowns_NegativeYardage_ChangesFieldPosition()
        {
            // Arrange - 4th and 2 at the 50, loses 1 yard (tackle for loss)
            var game = CreateGameAtDown(Downs.Fourth, 2, 50, Possession.Home);
            SetPlayerSkills(game, 70, 100); // Weak offense vs strong defense = negative yards
            var rng = CreateRngForRunPlay(-1, blockingSucceeds: false); // Tackled for loss!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "Should still be turnover on downs");
            Assert.AreEqual(49, game.FieldPosition, "Field position should be 50 + (-1) = 49");

            // Add to game.Plays so PrePlay can check previous play
            game.Plays.Add(game.CurrentPlay);

            // Simulate next play
            var nextPlayRng = new TestFluentSeedableRandom().NextDouble(0.3);
            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            // Assert - Away gets possession at the same field position (absolute position)
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession, "Away should get possession");
            Assert.AreEqual(49, game.FieldPosition, "Away starts at the 49 yard line (no flip)");
        }

        #endregion

        #region Pass Play Down Progression Tests

        [TestMethod]
        public void PassPlay_IncompletePass_AdvancesDown_NoYardageChange()
        {
            // Arrange - 2nd and 8, incomplete pass
            var game = CreateGameAtDownForPassPlay(Downs.Second, 8, 35, Possession.Home);
            SetPassPlayerSkills(game, 70, 70);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)        // Protection holds
                .NextDouble(0.99)                // Blocking penalty check (no penalty)
                .QBPressureCheck(0.5)            // No pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.9)        // INCOMPLETE (> 0.75)
                .NextDouble(0.99)                // Coverage penalty check (no penalty) - only on incomplete
                .InterceptionOccurredCheck(0.99) // No interception
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.AreEqual(Downs.Third, game.CurrentDown, "Incomplete pass should advance down");
            Assert.AreEqual(8, game.YardsToGo, "Yards to go should not change on incomplete");
            Assert.AreEqual(35, game.FieldPosition, "Field position should not change on incomplete");
        }

        [TestMethod]
        public void PassPlay_TurnoverOnDowns_FourthDownIncomplete()
        {
            // Arrange - 4th and 10, incomplete pass
            var game = CreateGameAtDownForPassPlay(Downs.Fourth, 10, 40, Possession.Home);
            SetPassPlayerSkills(game, 70, 70);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .NextDouble(0.99)                // Blocking penalty check (no penalty)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(15)
                .PassCompletionCheck(0.9)        // INCOMPLETE
                .NextDouble(0.99)                // Coverage penalty check (no penalty) - only on incomplete
                .InterceptionOccurredCheck(0.99) // No interception
                .ElapsedTimeRandomFactor(0.5);

            // Act
            var pass = new Pass(rng);
            pass.Execute(game);
            var passResult = new PassResult();
            passResult.Execute(game);

            // Assert
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "4th down incomplete should be turnover");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st for new possession");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards");

            // Add to game.Plays and verify possession flips
            game.Plays.Add(game.CurrentPlay);
            var nextPlayRng = new TestFluentSeedableRandom().NextDouble(0.3);
            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession, "Should flip to Away");
        }

        #endregion

        #region Field Position After Turnover Tests

        [TestMethod]
        public void TurnoverOnDowns_DeepInOwnTerritory_GivesOpponentGoodPosition()
        {
            // Arrange - Home has 4th down at their own 20, fails to convert
            var game = CreateGameAtDown(Downs.Fourth, 5, 20, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(3, blockingSucceeds: true); // Gains 3, needs 5

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(23, game.FieldPosition, "Ball should be at Home's 23");
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "Should be turnover");

            // Add to game.Plays so PrePlay can check previous play
            game.Plays.Add(game.CurrentPlay);

            // Simulate next play
            var nextPlayRng = new TestFluentSeedableRandom().NextDouble(0.3);
            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            // Assert - Away gets the ball in great field position
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession, "Away gets possession");
            Assert.AreEqual(23, game.FieldPosition, "Away starts at Home's 23 yard line");
        }

        #endregion

        #region Helper Methods

        private Game CreateGameAtDown(Downs down, int yardsToGo, int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.CurrentDown = down;
            game.YardsToGo = yardsToGo;
            game.FieldPosition = fieldPosition;

            // Create a run play at this down
            var runPlay = new RunPlay
            {
                Possession = possession,
                Down = down,
                StartFieldPosition = fieldPosition,
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

            return game;
        }

        private Game CreateGameAtDownForPassPlay(Downs down, int yardsToGo, int fieldPosition, Possession possession)
        {
            var game = _testGame.GetGame();
            game.CurrentDown = down;
            game.YardsToGo = yardsToGo;
            game.FieldPosition = fieldPosition;

            // Create a pass play at this down
            var passPlay = new PassPlay
            {
                Possession = possession,
                Down = down,
                StartFieldPosition = fieldPosition,
                ElapsedTime = 0
            };

            // Set offensive players
            passPlay.OffensePlayersOnField = new List<Player>
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
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][2]
            };

            // Set defensive players
            passPlay.DefensePlayersOnField = new List<Player>
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

            game.CurrentPlay = passPlay;

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

        private void SetPassPlayerSkills(Game game, int offenseSkill, int defenseSkill)
        {
            // Set offensive players
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = offenseSkill;
                player.Passing = offenseSkill;
                player.Catching = offenseSkill;
                player.Speed = offenseSkill;
                player.Agility = offenseSkill;
            }

            // Set defensive players
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = defenseSkill;
                player.Coverage = defenseSkill;
                player.Speed = defenseSkill;
                player.Strength = defenseSkill;
            }
        }

        private TestFluentSeedableRandom CreateRngForRunPlay(int desiredYards, bool blockingSucceeds)
        {
            // IMPORTANT: Two-step rounding process in the actual code:
            // Step 1: RunYardsSkillsCheckResult.Execute() does: Result = (int)Math.Round(totalYards)
            // Step 2: Run.Execute() does: adjustedYards = (int)(baseYards * blockingModifier)
            //
            // The rounding happens BEFORE the blocking modifier is applied!
            // So we must use Ceiling to account for this.

            double targetBase;
            if (blockingSucceeds)
            {
                // We need roundedBase such that (int)(roundedBase * 1.2) = desiredYards
                // Use Ceiling because Math.Round happens BEFORE the 1.2x modifier
                // Example: desiredYards=3 → 3/1.2=2.5 → Ceiling(2.5)=3 → Round(3)=3 → (int)(3*1.2)=3 ✓
                targetBase = Math.Ceiling(desiredYards / 1.2);
            }
            else
            {
                // We need roundedBase such that (int)(roundedBase * 0.8) = desiredYards
                // Use Ceiling because Math.Round happens BEFORE the 0.8x modifier
                targetBase = Math.Ceiling(desiredYards / 0.8);
            }

            // randomFactor = targetBase - 3.0
            double randomFactor = targetBase - 3.0;

            // Solve for NextDouble: randomFactor = (NextDouble * 25) - 15
            double nextDouble = (randomFactor + 15.0) / 25.0;
            nextDouble = Math.Max(0.0, Math.Min(1.0, nextDouble)); // Clamp to [0, 1]

            double blockingCheckValue = blockingSucceeds ? 0.3 : 0.7; // < 0.5 succeeds, >= 0.5 fails

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                     // QB check (RB)
                .NextInt(2)                           // Direction
                .RunBlockingCheck(blockingCheckValue) // Explicit blocking success/failure
                .NextDouble(0.99)                     // Blocking penalty check (no penalty)
                .NextDouble(nextDouble)               // Base yards calculation
                .TackleBreakCheck(0.9)                // No tackle break
                .BreakawayCheck(0.9)                  // No breakaway
                .NextDouble(0.99)                     // Tackle penalty check (no penalty)
                // Injury checks (ball carrier + 2 tacklers)
                .InjuryOccurredCheck(0.99)  // Ball carrier no injury
                .TacklerInjuryGateCheck(0.9)   // Tackler 1 skip
                .TacklerInjuryGateCheck(0.9)   // Tackler 2 skip
                .FumbleCheck(0.99)             // No fumble
                .OutOfBoundsCheck(0.99)        // Out of bounds check
                .ElapsedTimeRandomFactor(0.5);
        }

        private void ExecuteRunPlayWithResult(Game game, ISeedableRandom rng)
        {
            // Execute the run play
            var run = new Run(rng);
            run.Execute(game);

            // Execute the run result
            var runResult = new RunResult();
            runResult.Execute(game);
        }

        #endregion

        #region Comprehensive Skill x Blocking Matrix Tests

        [TestMethod]
        public void Matrix_HighSkills_GoodBlocking_ProducesHighYards()
        {
            // Arrange - Strong offense (90) vs weak defense (50), good blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 90, 50);
            var rng = CreateRngForRunPlay(10, blockingSucceeds: true);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should gain significant yards
            Assert.IsTrue(game.CurrentPlay.YardsGained >= 8,
                $"High skills + good blocking should produce high yards (got {game.CurrentPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_HighSkills_BadBlocking_ProducesMediumYards()
        {
            // Arrange - Strong offense (90) vs weak defense (50), bad blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 90, 50);
            var rng = CreateRngForRunPlay(6, blockingSucceeds: false);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should still gain decent yards despite bad blocking
            Assert.IsTrue(game.CurrentPlay.YardsGained >= 5,
                $"High skills + bad blocking should still produce medium yards (got {game.CurrentPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_EvenSkills_GoodBlocking_ProducesMediumYards()
        {
            // Arrange - Even matchup (70 vs 70), good blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(5, blockingSucceeds: true);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should gain medium yards
            Assert.IsTrue(game.CurrentPlay.YardsGained >= 4 && game.CurrentPlay.YardsGained <= 6,
                $"Even skills + good blocking should produce medium yards (got {game.CurrentPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_EvenSkills_BadBlocking_ProducesLowYards()
        {
            // Arrange - Even matchup (70 vs 70), bad blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 70, 70);
            var rng = CreateRngForRunPlay(2, blockingSucceeds: false);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should gain few yards
            Assert.IsTrue(game.CurrentPlay.YardsGained >= 1 && game.CurrentPlay.YardsGained <= 3,
                $"Even skills + bad blocking should produce low yards (got {game.CurrentPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_LowSkills_GoodBlocking_ProducesLowYards()
        {
            // Arrange - Weak offense (40) vs strong defense (80), good blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 40, 80);
            var rng = CreateRngForRunPlay(2, blockingSucceeds: true);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should gain minimal yards even with good blocking
            Assert.IsTrue(game.CurrentPlay.YardsGained <= 3,
                $"Low skills + good blocking should still produce low yards (got {game.CurrentPlay.YardsGained})");
        }

        [TestMethod]
        public void Matrix_LowSkills_BadBlocking_ProducesNegativeYards()
        {
            // Arrange - Weak offense (40) vs strong defense (80), bad blocking
            var game = CreateGameAtDown(Downs.First, 10, 30, Possession.Home);
            SetPlayerSkills(game, 40, 80);
            var rng = CreateRngForRunPlay(-2, blockingSucceeds: false);

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Should lose yards (tackle for loss)
            Assert.IsTrue(game.CurrentPlay.YardsGained <= 0,
                $"Low skills + bad blocking should produce tackle for loss (got {game.CurrentPlay.YardsGained})");
        }

        #endregion
    }
}
