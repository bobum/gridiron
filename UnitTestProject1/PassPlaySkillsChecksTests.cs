using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsChecks;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PassPlaySkillsChecksTests
    {
        private readonly Teams _teams = new Teams();
        private readonly TestGame _testGame = new TestGame();

        #region PassProtectionSkillsCheck Tests

        [TestMethod]
        public void PassProtectionSkillsCheck_StrongOffensiveLine_HighProtectionRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set offensive line to have strong blocking (85+)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T || player.Position == Positions.TE ||
                    player.Position == Positions.RB || player.Position == Positions.FB)
                {
                    player.Blocking = 90;
                }
            }

            // Set pass rush to be average (60)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.DT || player.Position == Positions.DE ||
                    player.Position == Positions.LB || player.Position == Positions.OLB)
                {
                    player.Tackling = 60;
                    player.Speed = 60;
                    player.Strength = 60;
                }
            }

            // Act & Assert - should succeed with ~90% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.85 } };
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            Assert.IsTrue(protectionCheck.Occurred);
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_WeakOffensiveLine_LowProtectionRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set offensive line to be weak (40)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T)
                {
                    player.Blocking = 40;
                }
            }

            // Set pass rush to be strong (85)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.DT || player.Position == Positions.DE ||
                    player.Position == Positions.LB || player.Position == Positions.OLB)
                {
                    player.Tackling = 85;
                    player.Speed = 85;
                    player.Strength = 85;
                }
            }

            // Act & Assert - should fail with ~52% protection probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.6 } };
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            Assert.IsFalse(protectionCheck.Occurred);
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_EvenMatchup_BaseRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set both offense and defense to be equal (70)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            // Act & Assert - 75% base probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.74 } };
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            Assert.IsTrue(protectionCheck.Occurred);

            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.76 } };
            protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            Assert.IsFalse(protectionCheck.Occurred);
        }

        #endregion

        #region QBPressureSkillsCheck Tests

        [TestMethod]
        public void QBPressureSkillsCheck_StrongPassRush_HighPressureRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set pass rush to be elite (90)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.DT || player.Position == Positions.DE ||
                    player.Position == Positions.LB || player.Position == Positions.OLB)
                {
                    player.Speed = 90;
                    player.Strength = 90;
                }
            }

            // Set O-line to be average (60)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T)
                {
                    player.Blocking = 60;
                }
            }

            // Act & Assert - should succeed with ~42% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.40 } };
            var pressureCheck = new QBPressureSkillsCheck(rng);
            pressureCheck.Execute(game);

            Assert.IsTrue(pressureCheck.Occurred);
        }

        [TestMethod]
        public void QBPressureSkillsCheck_WeakPassRush_LowPressureRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set pass rush to be weak (50)
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.DT || player.Position == Positions.DE ||
                    player.Position == Positions.LB || player.Position == Positions.OLB)
                {
                    player.Speed = 50;
                    player.Strength = 50;
                }
            }

            // Set O-line to be strong (85)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                if (player.Position == Positions.C || player.Position == Positions.G ||
                    player.Position == Positions.T)
                {
                    player.Blocking = 85;
                }
            }

            // Act & Assert - should fail with ~16% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.20 } };
            var pressureCheck = new QBPressureSkillsCheck(rng);
            pressureCheck.Execute(game);

            Assert.IsFalse(pressureCheck.Occurred);
        }

        [TestMethod]
        public void QBPressureSkillsCheck_EvenMatchup_BaseRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();

            // Set both to be equal (70)
            foreach (var player in game.CurrentPlay.OffensePlayersOnField)
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                player.Speed = 70;
                player.Strength = 70;
            }

            // Act & Assert - 30% base probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.29 } };
            var pressureCheck = new QBPressureSkillsCheck(rng);
            pressureCheck.Execute(game);

            Assert.IsTrue(pressureCheck.Occurred);

            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.31 } };
            pressureCheck = new QBPressureSkillsCheck(rng);
            pressureCheck.Execute(game);

            Assert.IsFalse(pressureCheck.Occurred);
        }

        #endregion

        #region PassCompletionSkillsCheck Tests

        [TestMethod]
        public void PassCompletionSkillsCheck_EliteQBAndReceiver_HighCompletionRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var qb = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.QB);
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);

            // Elite QB
            qb.Passing = 95;
            qb.Awareness = 90;

            // Elite receiver
            receiver.Catching = 95;
            receiver.Speed = 92;
            receiver.Agility = 90;

            // Average coverage
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.CB || player.Position == Positions.S ||
                    player.Position == Positions.FS || player.Position == Positions.LB)
                {
                    player.Coverage = 65;
                    player.Speed = 65;
                    player.Awareness = 65;
                }
            }

            // Act & Assert - should succeed with ~72% probability (no pressure)
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.70 } };
            var completionCheck = new PassCompletionSkillsCheck(rng, qb, receiver, false);
            completionCheck.Execute(game);

            Assert.IsTrue(completionCheck.Occurred);
        }

        [TestMethod]
        public void PassCompletionSkillsCheck_PoorQBAndReceiver_LowCompletionRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var qb = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.QB);
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);

            // Poor QB
            qb.Passing = 45;
            qb.Awareness = 40;

            // Poor receiver
            receiver.Catching = 50;
            receiver.Speed = 60;
            receiver.Agility = 55;

            // Elite coverage
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.CB || player.Position == Positions.S ||
                    player.Position == Positions.FS)
                {
                    player.Coverage = 90;
                    player.Speed = 88;
                    player.Awareness = 85;
                }
            }

            // Act & Assert - should fail with ~44% probability (no pressure)
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.50 } };
            var completionCheck = new PassCompletionSkillsCheck(rng, qb, receiver, false);
            completionCheck.Execute(game);

            Assert.IsFalse(completionCheck.Occurred);
        }

        [TestMethod]
        public void PassCompletionSkillsCheck_WithPressure_ReducedCompletionRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var qb = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.QB);
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);

            // Good QB and receiver
            qb.Passing = 80;
            qb.Awareness = 75;
            receiver.Catching = 80;
            receiver.Speed = 78;
            receiver.Agility = 75;

            // Average coverage
            foreach (var player in game.CurrentPlay.DefensePlayersOnField)
            {
                if (player.Position == Positions.CB || player.Position == Positions.S ||
                    player.Position == Positions.FS || player.Position == Positions.LB) // Add LB here
                {
                    player.Coverage = 70;
                    player.Speed = 70;
                    player.Awareness = 70;
                }
            }

            // Act & Assert - without pressure: ~63% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.62 } };
            var completionCheck = new PassCompletionSkillsCheck(rng, qb, receiver, false);
            completionCheck.Execute(game);

            Assert.IsTrue(completionCheck.Occurred);

            // With pressure: ~43% probability (20% reduction)
            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.44 } };
            completionCheck = new PassCompletionSkillsCheck(rng, qb, receiver, true);
            completionCheck.Execute(game);

            Assert.IsFalse(completionCheck.Occurred);
        }

        #endregion

        #region YardsAfterCatchSkillsCheck Tests

        [TestMethod]
        public void YardsAfterCatchSkillsCheck_EliteReceiver_HighYACRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);

            // Elite receiver
            receiver.Speed = 95;
            receiver.Agility = 92;
            receiver.Rushing = 88;

            // Act & Assert - should succeed with ~41% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.40 } };
            var yacCheck = new YardsAfterCatchSkillsCheck(rng, receiver);
            yacCheck.Execute(game);

            Assert.IsTrue(yacCheck.Occurred);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheck_SlowReceiver_LowYACRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.TE);

            // Slow, possession receiver
            receiver.Speed = 65;
            receiver.Agility = 60;
            receiver.Rushing = 55;

            // Act & Assert - should fail with ~32% probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.35 } };
            var yacCheck = new YardsAfterCatchSkillsCheck(rng, receiver);
            yacCheck.Execute(game);

            Assert.IsFalse(yacCheck.Occurred);
        }

        [TestMethod]
        public void YardsAfterCatchSkillsCheck_AverageReceiver_BaseRate()
        {
            // Arrange
            var game = CreateGameWithPassPlay();
            var receiver = game.CurrentPlay.OffensePlayersOnField.Find(p => p.Position == Positions.WR);

            // Average receiver
            receiver.Speed = 75;
            receiver.Agility = 73;
            receiver.Rushing = 70;

            // Act & Assert - ~35% base probability
            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.34 } };
            var yacCheck = new YardsAfterCatchSkillsCheck(rng, receiver);
            yacCheck.Execute(game);

            Assert.IsTrue(yacCheck.Occurred);

            rng = new TestSeedableRandom { __NextDouble = { [0] = 0.36 } };
            yacCheck = new YardsAfterCatchSkillsCheck(rng, receiver);
            yacCheck.Execute(game);

            Assert.IsFalse(yacCheck.Occurred);
        }

        #endregion

        #region Helper Methods

        private Game CreateGameWithPassPlay()
        {
            var game = _testGame.GetGame();

            // Create a pass play with proper formations
            var passPlay = new PassPlay
            {
                Possession = Possession.Home,
                Down = Downs.First,
                StartFieldPosition = 25
            };

            // Set offensive players (pass formation: 1 RB, 3 WR, 1 TE)
            passPlay.OffensePlayersOnField = new List<Player>
            {
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.RB][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.C][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.G][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.T][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][0],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][1],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.WR][2],
                _teams.HomeTeam.OffenseDepthChart.Chart[Positions.TE][0]
            };

            // Set defensive players (nickel defense: 4 DL, 2 LB, 2 CB, 2 S, 1 FS)
            passPlay.DefensePlayersOnField = new List<Player>
            {
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DE][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.DT][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.LB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.CB][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][0],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.S][1],
                _teams.VisitorTeam.DefenseDepthChart.Chart[Positions.FS][0]
            };

            game.CurrentPlay = passPlay;
            game.FieldPosition = 25;
            game.YardsToGo = 10;
            game.CurrentDown = Downs.First;

            return game;
        }

        #endregion

        #region Margin Property Tests

        [TestMethod]
        public void PassProtectionSkillsCheck_DecisiveSuccess_PositiveMargin()
        {
            // Arrange - Protection probability ~75%, roll = 0.20 (way under threshold)
            var game = CreateGameWithPassPlay();

            // Set equal skills for ~75% protection probability
            foreach (var player in game.CurrentPlay.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C || p.Position == Positions.G || p.Position == Positions.T))
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DE || p.Position == Positions.DT))
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.20 } };

            // Act
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            // Assert - Should succeed with large positive margin
            Assert.IsTrue(protectionCheck.Occurred);
            Assert.IsTrue(protectionCheck.Margin > 40); // (0.75 - 0.20) * 100 = 55
            Assert.IsTrue(protectionCheck.Margin < 70);
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_CloseSuccess_SmallPositiveMargin()
        {
            // Arrange - Protection probability ~75%, roll = 0.73 (just under threshold)
            var game = CreateGameWithPassPlay();

            foreach (var player in game.CurrentPlay.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C || p.Position == Positions.G || p.Position == Positions.T))
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DE || p.Position == Positions.DT))
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.73 } };

            // Act
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            // Assert - Should succeed with small positive margin
            Assert.IsTrue(protectionCheck.Occurred);
            Assert.IsTrue(protectionCheck.Margin > 0);
            Assert.IsTrue(protectionCheck.Margin < 10); // (0.75 - 0.73) * 100 = 2
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_CloseFailure_SmallNegativeMargin()
        {
            // Arrange - Protection probability ~75%, roll = 0.77 (just over threshold)
            var game = CreateGameWithPassPlay();

            foreach (var player in game.CurrentPlay.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C || p.Position == Positions.G || p.Position == Positions.T))
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DE || p.Position == Positions.DT))
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.77 } };

            // Act
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            // Assert - Should fail with small negative margin
            Assert.IsFalse(protectionCheck.Occurred);
            Assert.IsTrue(protectionCheck.Margin < 0);
            Assert.IsTrue(protectionCheck.Margin > -10); // (0.75 - 0.77) * 100 = -2
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_DecisiveFailure_LargeNegativeMargin()
        {
            // Arrange - Protection probability ~75%, roll = 0.95 (way over threshold)
            var game = CreateGameWithPassPlay();

            foreach (var player in game.CurrentPlay.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C || p.Position == Positions.G || p.Position == Positions.T))
            {
                player.Blocking = 70;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DE || p.Position == Positions.DT))
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.95 } };

            // Act
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            // Assert - Should fail with large negative margin
            Assert.IsFalse(protectionCheck.Occurred);
            Assert.IsTrue(protectionCheck.Margin < -15); // (0.75 - 0.95) * 100 = -20
            Assert.IsTrue(protectionCheck.Margin > -30);
        }

        [TestMethod]
        public void PassProtectionSkillsCheck_StrongOLine_HigherMarginForSuccess()
        {
            // Arrange - Strong O-Line (90) vs average D-Line (70) = high protection probability
            var game = CreateGameWithPassPlay();

            foreach (var player in game.CurrentPlay.OffensePlayersOnField.Where(p =>
                p.Position == Positions.C || p.Position == Positions.G || p.Position == Positions.T))
            {
                player.Blocking = 90;
            }
            foreach (var player in game.CurrentPlay.DefensePlayersOnField.Where(p =>
                p.Position == Positions.DE || p.Position == Positions.DT))
            {
                player.Tackling = 70;
                player.Speed = 70;
                player.Strength = 70;
            }

            TestSeedableRandom rng = new TestSeedableRandom { __NextDouble = { [0] = 0.50 } };

            // Act
            var protectionCheck = new PassProtectionSkillsCheck(rng);
            protectionCheck.Execute(game);

            // Assert - Should succeed with high margin (better O-Line = higher probability)
            Assert.IsTrue(protectionCheck.Occurred);
            Assert.IsTrue(protectionCheck.Margin > 25); // Probability should be ~85%, margin ~35
        }

        #endregion
    }
}
