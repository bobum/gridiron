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

            // Act & Assert - should succeed with high probability (~67.5% with +35 skill differential)
            var rng = new TestFluentSeedableRandom()
                .RunBlockingCheck(0.6); // 0.6 < 0.675 probability = blocking succeeds

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

            // Act & Assert - should fail (0.3 > 0.25 probability with -50 skill differential)
            var rng = new TestFluentSeedableRandom()
                .RunBlockingCheck(0.3); // 0.3 > 0.25 probability = blocking fails

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

            // Act & Assert - 50% probability (even matchup)
            var rng = new TestFluentSeedableRandom()
                .RunBlockingCheck(0.49); // 0.49 < 0.50 = succeeds

            var blockingCheck = new BlockingSuccessSkillsCheck(rng);
            blockingCheck.Execute(game);

            Assert.IsTrue(blockingCheck.Occurred);

            // Try with just over 50%
            rng = new TestFluentSeedableRandom()
                .RunBlockingCheck(0.51); // 0.51 > 0.50 = fails

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

            // Act & Assert - should succeed with ~40% probability (+33.3 skill differential)
            var rng = new TestFluentSeedableRandom()
                .TackleBreakCheck(0.35); // 0.35 < ~0.38 probability = tackle break succeeds

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

            // Act & Assert - should fail (0.08 > ~0.05-0.07 probability with -45 skill differential)
            var rng = new TestFluentSeedableRandom()
                .TackleBreakCheck(0.08); // 0.08 > low probability = tackle break fails

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

            // Act & Assert - ~25% probability (even matchup)
            var rng = new TestFluentSeedableRandom()
                .TackleBreakCheck(0.24); // 0.24 < 0.25 = succeeds

            var tackleBreakCheck = new TackleBreakSkillsCheck(rng, ballCarrier);
            tackleBreakCheck.Execute(game);

            Assert.IsTrue(tackleBreakCheck.Occurred);

            rng = new TestFluentSeedableRandom()
                .TackleBreakCheck(0.26); // 0.26 > 0.25 = fails

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

            // Very fast ball carrier (speed 95 gives ~13% probability)
            ballCarrier.Speed = 95;

            // Act & Assert - should succeed with ~13% probability
            var rng = new TestFluentSeedableRandom()
                .BreakawayCheck(0.12); // 0.12 < 0.13 probability = big run succeeds

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

            // Slow ball carrier (speed 50 gives ~4% probability)
            ballCarrier.Speed = 50;

            // Act & Assert - should have ~4% probability
            var rng = new TestFluentSeedableRandom()
                .BreakawayCheck(0.05); // 0.05 > 0.04 probability = big run fails

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

            // Average speed (70 gives base 8% probability)
            ballCarrier.Speed = 70;

            // Act & Assert - base 8% probability
            var rng = new TestFluentSeedableRandom()
                .BreakawayCheck(0.07); // 0.07 < 0.08 = succeeds

            var bigRunCheck = new BigRunSkillsCheck(rng, ballCarrier);
            bigRunCheck.Execute(game);

            Assert.IsTrue(bigRunCheck.Occurred);

            rng = new TestFluentSeedableRandom()
                .BreakawayCheck(0.09); // 0.09 > 0.08 = fails

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