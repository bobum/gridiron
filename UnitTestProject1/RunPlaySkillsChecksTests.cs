using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsChecks;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class RunPlaySkillsChecksTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region BlockingSuccessSkillsCheck Tests

        [TestMethod]
        public void BlockingSuccessSkillsCheck_StrongOffensiveLine_HighSuccessRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();

            // Set offensive line to have strong blocking (80+)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T || player.Position == Positions.TE ||
                    player.Position == Positions.FB)
                {
                    player.Blocking = 85;
                }
            }

            // Set defense to be average (50)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = 50;
                player.Strength = 50;
            }

            // Act & Assert - should succeed with high probability (0.675 probability)
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.6 } };
            var blockingCheck = new BlockingSuccessSkillsCheck(rng);
            blockingCheck.Execute(game);

            Assert.IsTrue(blockingCheck.Occurred);
        }

        [TestMethod]
        public void BlockingSuccessSkillsCheck_WeakOffensiveLine_LowSuccessRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();

            // Set offensive line to be weak (30)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T || player.Position == Positions.TE ||
                    player.Position == Positions.FB)
                {
                    player.Blocking = 30;
                }
            }

            // Set defense to be strong (80)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.DT || player.Position == Positions.DE ||
                    player.Position == Positions.LB)
                {
                    player.Tackling = 80;
                    player.Strength = 80;
                }
            }

            // Act & Assert - should fail with low random value (0.25 probability)
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.3 } };
            var blockingCheck = new BlockingSuccessSkillsCheck(rng);
            blockingCheck.Execute(game);

            Assert.IsFalse(blockingCheck.Occurred);
        }

        [TestMethod]
        public void BlockingSuccessSkillsCheck_EvenMatchup_FiftyFifty()
        {
            // Arrange
            var game = CreateGameWithRunPlay();

            // Set both offense and defense to be equal (60)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = 60;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = 60;
                player.Strength = 60;
            }

            // Act & Assert - 50% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.49 } };
            var blockingCheck = new BlockingSuccessSkillsCheck(rng);
            blockingCheck.Execute(game);

            Assert.IsTrue(blockingCheck.Occurred);

            // Try with just over 50%
            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.51 } };
            blockingCheck = new BlockingSuccessSkillsCheck(rng);
            blockingCheck.Execute(game);

            Assert.IsFalse(blockingCheck.Occurred);
        }

        #endregion

        #region TackleBreakSkillsCheck Tests

        [TestMethod]
        public void TackleBreakSkillsCheck_EliteBallCarrier_HighBreakRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Elite ball carrier
            ballCarrier.Rushing = 95;
            ballCarrier.Strength = 90;
            ballCarrier.Agility = 95;

            // Average defense
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = 60;
                player.Strength = 60;
                player.Speed = 60;
            }

            // Act & Assert - should succeed with ~40% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.35 } };
            var tackleBreakCheck = new TackleBreakSkillsCheck(rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            Assert.IsTrue(tackleBreakCheck.Occurred);
        }

        [TestMethod]
        public void TackleBreakSkillsCheck_WeakBallCarrier_LowBreakRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Weak ball carrier
            ballCarrier.Rushing = 40;
            ballCarrier.Strength = 35;
            ballCarrier.Agility = 40;

            // Strong defense
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.LB || player.Position == Positions.DE ||
                    player.Position == Positions.DT || player.Position == Positions.CB ||
                    player.Position == Positions.S || player.Position == Positions.FS)
                {
                    player.Tackling = 85;
                    player.Strength = 85;
                    player.Speed = 80;
                }
            }

            // Act & Assert - should fail (very low probability ~5-10%)
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.08 } };
            var tackleBreakCheck = new TackleBreakSkillsCheck(rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            Assert.IsFalse(tackleBreakCheck.Occurred);
        }

        [TestMethod]
        public void TackleBreakSkillsCheck_AverageBallCarrier_ModerateBreakRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Average ball carrier
            ballCarrier.Rushing = 70;
            ballCarrier.Strength = 70;
            ballCarrier.Agility = 70;

            // Average defense
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = 70;
                player.Strength = 70;
                player.Speed = 70;
            }

            // Act & Assert - ~25% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.24 } };
            var tackleBreakCheck = new TackleBreakSkillsCheck(rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            Assert.IsTrue(tackleBreakCheck.Occurred);

            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.26 } };
            tackleBreakCheck = new TackleBreakSkillsCheck(rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            Assert.IsFalse(tackleBreakCheck.Occurred);
        }

        #endregion

        #region BigRunSkillsCheck Tests

        [TestMethod]
        public void BigRunSkillsCheck_HighSpeedBallCarrier_HighBreakawayRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Very fast ball carrier
            ballCarrier.Speed = 95;

            // Act & Assert - should succeed with ~13% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.12 } };
            var bigRunCheck = new BigRunSkillsCheck(rng, ballCarrier);
            bigRunCheck.Execute(game);

            Assert.IsTrue(bigRunCheck.Occurred);
        }

        [TestMethod]
        public void BigRunSkillsCheck_LowSpeedBallCarrier_LowBreakawayRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Slow ball carrier
            ballCarrier.Speed = 50;

            // Act & Assert - should have ~4% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.05 } };
            var bigRunCheck = new BigRunSkillsCheck(rng, ballCarrier);
            bigRunCheck.Execute(game);

            Assert.IsFalse(bigRunCheck.Occurred);
        }

        [TestMethod]
        public void BigRunSkillsCheck_AverageSpeedBallCarrier_BaseRate()
        {
            // Arrange
            var game = CreateGameWithRunPlay();
            var ballCarrier = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.RB);

            // Average speed
            ballCarrier.Speed = 70;

            // Act & Assert - base 8% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.07 } };
            var bigRunCheck = new BigRunSkillsCheck(rng, ballCarrier);
            bigRunCheck.Execute(game);

            Assert.IsTrue(bigRunCheck.Occurred);

            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.09 } };
            bigRunCheck = new BigRunSkillsCheck(rng, ballCarrier);
            bigRunCheck.Execute(game);

            Assert.IsFalse(bigRunCheck.Occurred);
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
                StartFieldPosition = 25
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

        #endregion
    }
}
