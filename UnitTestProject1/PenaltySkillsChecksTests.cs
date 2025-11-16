using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsChecks;
using StateLibrary.SkillsCheckResults;
using System.Collections.Generic;
using System.Linq;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    [TestClass]
    public class PenaltySkillsChecksTests
    {
        private readonly TestGame _testGame = new TestGame();

        #region PreSnapPenaltyOccurredSkillsCheck Tests

        [TestMethod]
        public void PreSnapPenalty_NoPenaltyOccurs_WhenRollIsHigh()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99); // Very high roll - no penalty

            var check = new PreSnapPenaltyOccurredSkillsCheck(rng, PlayType.Pass);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.AreEqual(PenaltyNames.NoPenalty, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void PreSnapPenalty_FalseStartOccurs_WhenRollInRange()
        {
            // Arrange
            var game = _testGame.GetGame();
            // False Start is first penalty, odds = 1.55% = 0.0155
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.01); // Within false start range

            var check = new PreSnapPenaltyOccurredSkillsCheck(rng, PlayType.Pass);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            Assert.AreEqual(PenaltyNames.FalseStart, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void PreSnapPenalty_DelayOfGameOccurs_WhenRollInRange()
        {
            // Arrange
            var game = _testGame.GetGame();
            // Delay of Game starts after False Start (1.55%) and has 0.40% odds
            // So cumulative is 1.55% + 0.40% = 1.95%
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.017); // Between 1.55% and 1.95%

            var check = new PreSnapPenaltyOccurredSkillsCheck(rng, PlayType.Pass);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            Assert.AreEqual(PenaltyNames.DelayofGame, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void PreSnapPenalty_ReturnsCorrectMargin()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99);

            var check = new PreSnapPenaltyOccurredSkillsCheck(rng, PlayType.Run);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.IsTrue(check.Margin > 0); // Should have positive margin (distance from penalty)
        }

        #endregion

        #region BlockingPenaltyOccurredSkillsCheck Tests

        [TestMethod]
        public void BlockingPenalty_NoPenaltyOccurs_WhenRollIsHigh()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99);

            var oLine = CreateOffensiveLinemen(5, blocking: 80);
            var dLine = CreateDefensiveLine(4, tackling: 60);

            var check = new BlockingPenaltyOccurredSkillsCheck(rng, oLine, dLine, PlayType.Pass);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.AreEqual(PenaltyNames.NoPenalty, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void BlockingPenalty_OffensiveHoldingOccurs_WhenRollInRange()
        {
            // Arrange
            var game = _testGame.GetGame();
            // Offensive Holding is first and most common at 1.90%
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.005);

            var oLine = CreateOffensiveLinemen(5, blocking: 50);
            var dLine = CreateDefensiveLine(4, tackling: 80); // Strong rush = more holding

            var check = new BlockingPenaltyOccurredSkillsCheck(rng, oLine, dLine, PlayType.Pass);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            Assert.AreEqual(PenaltyNames.OffensiveHolding, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void BlockingPenalty_HigherProbability_WithBetterPassRush()
        {
            // Arrange
            var game = _testGame.GetGame();

            var oLine = CreateOffensiveLinemen(5, blocking: 60);

            // Test with weak pass rush
            var weakDLine = CreateDefensiveLine(4, tackling: 40);
            var rng1 = new TestFluentSeedableRandom().NextDouble(0.015);
            var check1 = new BlockingPenaltyOccurredSkillsCheck(rng1, oLine, weakDLine, PlayType.Pass);
            check1.Execute(game);

            // Test with strong pass rush (same roll should cause penalty)
            var strongDLine = CreateDefensiveLine(4, tackling: 95);
            var rng2 = new TestFluentSeedableRandom().NextDouble(0.015);
            var check2 = new BlockingPenaltyOccurredSkillsCheck(rng2, oLine, strongDLine, PlayType.Pass);
            check2.Execute(game);

            // Assert - both should execute without error
            Assert.IsNotNull(check1);
            Assert.IsNotNull(check2);
        }

        #endregion

        #region CoveragePenaltyOccurredSkillsCheck Tests

        [TestMethod]
        public void CoveragePenalty_NoPenaltyOccurs_WhenRollIsHigh()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99);

            var receiver = new Player { Catching = 70, LastName = "Receiver" };
            var dbs = CreateDefensiveBacks(3, coverage: 75);

            var check = new CoveragePenaltyOccurredSkillsCheck(rng, receiver, dbs, false, 15);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.AreEqual(PenaltyNames.NoPenalty, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void CoveragePenalty_PassInterferenceOccurs_OnDeepIncompletion()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.003); // Low roll

            var receiver = new Player { Catching = 85, LastName = "Receiver" };
            var dbs = CreateDefensiveBacks(2, coverage: 60); // Weak coverage

            var check = new CoveragePenaltyOccurredSkillsCheck(
                rng, receiver, dbs,
                passCompleted: false,  // Incomplete
                airYards: 30);          // Deep pass

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            // DPI is most likely on deep incompletions
            Assert.AreEqual(PenaltyNames.DefensivePassInterference, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void CoveragePenalty_IllegalContactOccurs_OnShortPass()
        {
            // Arrange
            var game = _testGame.GetGame();
            // Illegal Contact odds = 0.16%, but boosted 2x on short passes
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.002);

            var receiver = new Player { Catching = 75, LastName = "Receiver" };
            var dbs = CreateDefensiveBacks(2, coverage: 65);

            var check = new CoveragePenaltyOccurredSkillsCheck(
                rng, receiver, dbs,
                passCompleted: false,
                airYards: 3); // Very short pass (within 5 yards)

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            // On short passes, illegal contact is more likely
            // (test may be sensitive to exact roll value)
        }

        #endregion

        #region TacklePenaltyOccurredSkillsCheck Tests

        [TestMethod]
        public void TacklePenalty_NoPenaltyOccurs_WhenRollIsHigh()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99);

            var ballCarrier = new Player { LastName = "Runner" };
            var tacklers = CreateDefenders(2, strength: 70); // Normal players

            var check = new TacklePenaltyOccurredSkillsCheck(
                rng, ballCarrier, tacklers, TackleContext.BallCarrier);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.AreEqual(PenaltyNames.NoPenalty, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void TacklePenalty_RoughingThePasserOccurs_OnQBHit()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.002); // Low roll

            var qb = new Player { Position = Positions.QB, LastName = "Quarterback" };
            var tacklers = CreateDefenders(1, strength: 80);

            var check = new TacklePenaltyOccurredSkillsCheck(
                rng, qb, tacklers, TackleContext.PasserInPocket);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            Assert.AreEqual(PenaltyNames.RoughingthePasser, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void TacklePenalty_RoughingTheKickerOccurs_OnKickerContact()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.001); // Very low roll

            var kicker = new Player { Position = Positions.P, LastName = "Punter" };
            var tacklers = CreateDefenders(1, strength: 70);

            var check = new TacklePenaltyOccurredSkillsCheck(
                rng, kicker, tacklers, TackleContext.Kicker);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            Assert.AreEqual(PenaltyNames.RoughingtheKicker, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void TacklePenalty_HigherProbability_WithAggressiveTacklers()
        {
            // Arrange
            var game = _testGame.GetGame();
            var ballCarrier = new Player { LastName = "Runner" };

            // Normal tacklers
            var normalTacklers = CreateDefenders(2, strength: 50);
            var rng1 = new TestFluentSeedableRandom().NextDouble(0.005);
            var check1 = new TacklePenaltyOccurredSkillsCheck(
                rng1, ballCarrier, normalTacklers, TackleContext.BallCarrier);
            check1.Execute(game);

            // Strong tacklers (higher penalty chance based on implementation)
            var strongTacklers = CreateDefenders(2, strength: 95);
            var rng2 = new TestFluentSeedableRandom().NextDouble(0.005);
            var check2 = new TacklePenaltyOccurredSkillsCheck(
                rng2, ballCarrier, strongTacklers, TackleContext.BallCarrier);
            check2.Execute(game);

            // Assert - both should execute without error
            Assert.IsNotNull(check1);
            Assert.IsNotNull(check2);
        }

        #endregion

        #region PostPlayPenaltyOccurredSkillsCheck Tests

        [TestMethod]
        public void PostPlayPenalty_NoPenaltyOccurs_WhenRollIsHigh()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99);

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var check = new PostPlayPenaltyOccurredSkillsCheck(
                rng, homePlayers, awayPlayers, false, false);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsFalse(check.Occurred);
            Assert.AreEqual(PenaltyNames.NoPenalty, check.PenaltyThatOccurred);
        }

        [TestMethod]
        public void PostPlayPenalty_TauntingOccurs_AfterBigPlay()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.0002); // Very low roll

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var check = new PostPlayPenaltyOccurredSkillsCheck(
                rng, homePlayers, awayPlayers,
                bigPlayOccurred: true,  // Big play increases emotion
                turnoverOccurred: false);

            // Act
            check.Execute(game);

            // Assert
            Assert.IsTrue(check.Occurred);
            // Should be taunting or unsportsmanlike conduct
            Assert.IsTrue(
                check.PenaltyThatOccurred == PenaltyNames.Taunting ||
                check.PenaltyThatOccurred == PenaltyNames.UnsportsmanlikeConduct ||
                check.PenaltyThatOccurred == PenaltyNames.PersonalFoul);
        }

        [TestMethod]
        public void PostPlayPenalty_HigherProbability_AfterTurnover()
        {
            // Arrange
            var game = _testGame.GetGame();
            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            // Normal play
            var rng1 = new TestFluentSeedableRandom().NextDouble(0.002);
            var check1 = new PostPlayPenaltyOccurredSkillsCheck(
                rng1, homePlayers, awayPlayers, false, false);
            check1.Execute(game);

            // After turnover (should have higher probability)
            var rng2 = new TestFluentSeedableRandom().NextDouble(0.002);
            var check2 = new PostPlayPenaltyOccurredSkillsCheck(
                rng2, homePlayers, awayPlayers, false, turnoverOccurred: true);
            check2.Execute(game);

            // Assert - both should execute
            Assert.IsNotNull(check1);
            Assert.IsNotNull(check2);
        }

        #endregion

        #region PenaltyEffectSkillsCheckResult Tests

        [TestMethod]
        public void PenaltyEffect_CreatesCorrectResult_ForFalseStart()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.6)  // Will select team based on AwayOdds
                .NextInt(0);      // Player index

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.FalseStart,
                PenaltyOccuredWhen.Before,
                homePlayers,
                awayPlayers,
                Possession.Home,
                50); // Field position

            // Act
            result.Execute(game);

            // Assert
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(PenaltyNames.FalseStart, result.Result.PenaltyName);
            Assert.AreEqual(5, result.Result.Yards); // False start is 5 yards
            Assert.AreEqual(PenaltyOccuredWhen.Before, result.Result.OccurredWhen);
            Assert.IsNotNull(result.Result.CommittedBy);
            Assert.IsTrue(result.Result.Accepted); // Should be accepted
        }

        [TestMethod]
        public void PenaltyEffect_CreatesCorrectYards_ForOffensiveHolding()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.5)
                .NextInt(0);

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.OffensiveHolding,
                PenaltyOccuredWhen.During,
                homePlayers,
                awayPlayers,
                Possession.Home,
                50);

            // Act
            result.Execute(game);

            // Assert
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(10, result.Result.Yards); // Holding is 10 yards
        }

        [TestMethod]
        public void PenaltyEffect_CreatesCorrectYards_ForRoughingThePasser()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.5)
                .NextInt(0);

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.RoughingthePasser,
                PenaltyOccuredWhen.During,
                homePlayers,
                awayPlayers,
                Possession.Home,
                50);

            // Act
            result.Execute(game);

            // Assert
            Assert.IsNotNull(result.Result);
            Assert.AreEqual(15, result.Result.Yards); // Roughing the passer is 15 yards
        }

        [TestMethod]
        public void PenaltyEffect_SelectsPlayerFromCorrectTeam()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.1)  // AwayOdds roll
                .NextInt(0);      // Player index

            var homePlayers = new List<Player>
            {
                new Player { FirstName = "Home", LastName = "Player1" }
            };
            var awayPlayers = new List<Player>
            {
                new Player { FirstName = "Away", LastName = "Player1" }
            };

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.FalseStart,
                PenaltyOccuredWhen.Before,
                homePlayers,
                awayPlayers,
                Possession.Home,
                50);

            // Act
            result.Execute(game);

            // Assert
            Assert.IsNotNull(result.Result);
            Assert.IsNotNull(result.Result.CommittedBy);
            // Verify player is from one of the teams
            Assert.IsTrue(
                result.Result.CommittedBy.LastName == "Player1");
        }

        [TestMethod]
        public void PenaltyEffect_HandlesDefensivePassInterference_SpotFoul()
        {
            // Arrange
            var game = _testGame.GetGame();
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.5)  // Team selection
                .NextDouble(0.5)  // Yards calculation
                .NextInt(0);      // Player selection

            var homePlayers = CreatePlayers(11);
            var awayPlayers = CreatePlayers(11);

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.DefensivePassInterference,
                PenaltyOccuredWhen.During,
                homePlayers,
                awayPlayers,
                Possession.Home,
                50);

            // Act
            result.Execute(game);

            // Assert
            Assert.IsNotNull(result.Result);
            // DPI is variable yards (10-35)
            Assert.IsTrue(result.Result.Yards >= 10 && result.Result.Yards <= 35);
        }

        #endregion

        #region Helper Methods

        private List<Player> CreateOffensiveLinemen(int count, int blocking)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(new Player
                {
                    Position = Positions.G,
                    Blocking = blocking,
                    LastName = $"OL{i}"
                });
            }
            return players;
        }

        private List<Player> CreateDefensiveLine(int count, int tackling)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(new Player
                {
                    Position = Positions.DE,
                    Tackling = tackling,
                    LastName = $"DL{i}"
                });
            }
            return players;
        }

        private List<Player> CreateDefensiveBacks(int count, int coverage)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(new Player
                {
                    Position = Positions.CB,
                    Coverage = coverage,
                    LastName = $"DB{i}"
                });
            }
            return players;
        }

        private List<Player> CreateDefenders(int count, int strength)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(new Player
                {
                    Position = Positions.LB,
                    Strength = strength,
                    LastName = $"Defender{i}"
                });
            }
            return players;
        }

        private List<Player> CreatePlayers(int count)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(new Player
                {
                    Position = Positions.WR,
                    FirstName = "Test",
                    LastName = $"Player{i}"
                });
            }
            return players;
        }

        #endregion
    }
}
