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
            var rng = CreateRngForRunPlay(3); // 3 yards gained

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
            var rng = CreateRngForRunPlay(4); // 4 yards gained

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
            var rng = CreateRngForRunPlay(2); // 2 yards gained

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
            var rng = CreateRngForRunPlay(12); // 12 yards - more than enough!

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
            var rng = CreateRngForRunPlay(7); // Exactly 7 yards

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
            var rng = CreateRngForRunPlay(2); // Only 2 yards - not enough!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert - Play result should set turnover
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "PossessionChange should be true");
            Assert.AreEqual(47, game.FieldPosition, "Field position should be 45 + 2");
            Assert.AreEqual(Downs.First, game.CurrentDown, "Should reset to 1st down for new possession");
            Assert.AreEqual(10, game.YardsToGo, "Should reset to 10 yards to go");

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
            var rng = CreateRngForRunPlay(1); // Only 1 yard

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "PossessionChange should be true");
            Assert.AreEqual(61, game.FieldPosition, "Field position should be 60 + 1");

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
            // Arrange - 4th and 2 at the 50, loses 3 yards (tackle for loss)
            var game = CreateGameAtDown(Downs.Fourth, 2, 50, Possession.Home);
            var rng = CreateRngForRunPlay(-3); // Tackled for loss!

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "Should still be turnover on downs");
            Assert.AreEqual(47, game.FieldPosition, "Field position should be 50 + (-3) = 47");

            // Simulate next play
            var nextPlayRng = new TestFluentSeedableRandom().NextDouble(0.3);
            var prePlay = new PrePlay(nextPlayRng);
            prePlay.Execute(game);

            // Assert - Away gets great field position (at Home's 47)
            Assert.AreEqual(Possession.Away, game.CurrentPlay.Possession, "Away should get possession");
            Assert.AreEqual(47, game.FieldPosition, "Away starts at the 47 yard line");
        }

        #endregion

        #region Pass Play Down Progression Tests

        [TestMethod]
        public void PassPlay_IncompletePa_AdvancesDown_NoYardageChange()
        {
            // Arrange - 2nd and 8, incomplete pass
            var game = CreateGameAtDown(Downs.Second, 8, 35, Possession.Home);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)        // Protection holds
                .QBPressureCheck(0.5)            // No pressure
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)      // Forward pass
                .AirYards(10)
                .PassCompletionCheck(0.9)        // INCOMPLETE (> 0.75)
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
            var game = CreateGameAtDown(Downs.Fourth, 10, 40, Possession.Home);

            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(15)
                .PassCompletionCheck(0.9)        // INCOMPLETE
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
        }

        #endregion

        #region Field Position After Turnover Tests

        [TestMethod]
        public void TurnoverOnDowns_DeepInOwnTerritory_GivesOpponentGoodPosition()
        {
            // Arrange - Home has 4th down at their own 20, fails to convert
            var game = CreateGameAtDown(Downs.Fourth, 5, 20, Possession.Home);
            var rng = CreateRngForRunPlay(3); // Gains 3, needs 5

            // Act
            ExecuteRunPlayWithResult(game, rng);

            // Assert
            Assert.AreEqual(23, game.FieldPosition, "Ball should be at Home's 23");
            Assert.IsTrue(game.CurrentPlay.PossessionChange, "Should be turnover");

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

        private TestFluentSeedableRandom CreateRngForRunPlay(int desiredYards)
        {
            // Create RNG that will produce approximately the desired yardage
            // This is a simplified approach - actual yards will depend on player skills
            // Base calculation: baseYards = 3.0 + (skillDiff / 20.0) + randomFactor
            // randomFactor = (NextDouble() * 11) - 3
            // With even skills (diff = 0): baseYards ≈ 3.0 + randomFactor

            double randomFactor = desiredYards - 3.0; // Approximate
            double nextDouble = (randomFactor + 3.0) / 11.0; // Solve for NextDouble
            nextDouble = Math.Max(0.0, Math.Min(1.0, nextDouble)); // Clamp to [0, 1]

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                     // QB check (RB)
                .NextInt(2)                           // Direction
                .RunBlockingCheck(0.5)                // Blocking (50/50)
                .NextDouble(nextDouble)               // Base yards calculation
                .TackleBreakCheck(0.9)                // No tackle break
                .BreakawayCheck(0.9)                  // No breakaway
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
    }
}
