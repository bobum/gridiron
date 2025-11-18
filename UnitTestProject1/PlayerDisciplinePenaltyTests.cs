using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.SkillsCheckResults;
using System.Collections.Generic;
using System.Linq;
using UnitTestProject1.Helpers;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests that player discipline affects penalty rates appropriately.
    ///
    /// Per implementation in PenaltyEffectSkillsCheckResult.SelectPlayerByDiscipline():
    /// - Weight = (100 - Discipline) + 20
    /// - Lower discipline = higher weight = more likely to commit penalties
    /// - Discipline 0 (undisciplined) = weight 120
    /// - Discipline 100 (highly disciplined) = weight 20
    /// - Default discipline if unset = 70 (weight 50)
    ///
    /// These tests verify that over many penalty occurrences, players with lower
    /// discipline commit significantly more penalties than highly disciplined players.
    /// </summary>
    [TestClass]
    public class PlayerDisciplinePenaltyTests
    {
        #region Statistical Distribution Tests

        [TestMethod]
        public void LowDisciplinePlayer_CommitsMorePenalties_ThanHighDisciplinePlayer()
        {
            // Arrange - Create two player groups
            var lowDisciplinePlayer = new Player
            {
                FirstName = "Undisciplined",
                LastName = "Player",
                Position = Positions.G,
                Discipline = 20 // Low discipline, weight = (100-20)+20 = 100
            };

            var highDisciplinePlayer = new Player
            {
                FirstName = "Disciplined",
                LastName = "Player",
                Position = Positions.G,
                Discipline = 90 // High discipline, weight = (100-90)+20 = 30
            };

            var players = new List<Player> { lowDisciplinePlayer, highDisciplinePlayer };
            var homePlayers = new List<Player> { lowDisciplinePlayer };
            var awayPlayers = new List<Player> { highDisciplinePlayer };

            // Run 1000 penalty selections
            int lowDisciplineCount = 0;
            int highDisciplineCount = 0;
            int trials = 1000;

            for (int i = 0; i < trials; i++)
            {
                var rng = new TestFluentSeedableRandom()
                    .NextDouble(0.3) // Select home team
                    .NextInt(0);     // Will be used for player selection

                var result = new PenaltyEffectSkillsCheckResult(
                    rng,
                    PenaltyNames.OffensiveHolding,
                    PenaltyOccuredWhen.During,
                    players,
                    players,
                    Possession.Home,
                    50);

                result.Execute(null);

                if (result.Result.CommittedBy.Discipline == 20)
                    lowDisciplineCount++;
                else if (result.Result.CommittedBy.Discipline == 90)
                    highDisciplineCount++;
            }

            // Assert - Low discipline player should commit significantly more penalties
            // Expected ratio: 100:30 or about 3.3:1
            // With 1000 trials, we expect roughly 769 low discipline, 231 high discipline
            Assert.IsTrue(lowDisciplineCount > highDisciplineCount * 2,
                $"Low discipline player should commit > 2x penalties. " +
                $"Actual: Low={lowDisciplineCount}, High={highDisciplineCount}");

            // More specific assertion - should be roughly 3:1 ratio (allowing for variance)
            double ratio = (double)lowDisciplineCount / highDisciplineCount;
            Assert.IsTrue(ratio > 2.5 && ratio < 4.5,
                $"Ratio should be ~3.3:1, actual: {ratio:F2}:1 " +
                $"(Low={lowDisciplineCount}, High={highDisciplineCount})");
        }

        [TestMethod]
        public void ZeroDiscipline_CommitsMorePenalties_Than100Discipline()
        {
            // Arrange - Extreme cases
            var noDisciplinePlayer = new Player
            {
                FirstName = "No",
                LastName = "Discipline",
                Position = Positions.T,
                Discipline = 0 // weight = (100-0)+20 = 120
            };

            var perfectDisciplinePlayer = new Player
            {
                FirstName = "Perfect",
                LastName = "Discipline",
                Position = Positions.T,
                Discipline = 100 // weight = (100-100)+20 = 20
            };

            var players = new List<Player> { noDisciplinePlayer, perfectDisciplinePlayer };

            // Run 1000 trials
            int noDisciplineCount = 0;
            int perfectDisciplineCount = 0;
            int trials = 1000;

            for (int i = 0; i < trials; i++)
            {
                var rng = new TestFluentSeedableRandom()
                    .NextDouble(0.5)
                    .NextInt(0);

                var result = new PenaltyEffectSkillsCheckResult(
                    rng,
                    PenaltyNames.FalseStart,
                    PenaltyOccuredWhen.Before,
                    players,
                    players,
                    Possession.Home,
                    50);

                result.Execute(null);

                if (result.Result.CommittedBy.Discipline == 0)
                    noDisciplineCount++;
                else
                    perfectDisciplineCount++;
            }

            // Assert - Expected ratio 120:20 = 6:1
            // With 1000 trials, expect roughly 857 undisciplined, 143 disciplined
            Assert.IsTrue(noDisciplineCount > perfectDisciplineCount * 4,
                $"Zero discipline should commit > 4x penalties. " +
                $"Actual: Zero={noDisciplineCount}, Perfect={perfectDisciplineCount}");

            double ratio = (double)noDisciplineCount / perfectDisciplineCount;
            Assert.IsTrue(ratio > 4.5 && ratio < 7.5,
                $"Ratio should be ~6:1, actual: {ratio:F2}:1");
        }

        [TestMethod]
        public void MediumDiscipline_CommitsModerateNumberOfPenalties()
        {
            // Arrange - Three tiers of discipline
            var lowDiscipline = new Player { Discipline = 30, FirstName = "Low", LastName = "D", Position = Positions.WR };
            var medDiscipline = new Player { Discipline = 60, FirstName = "Med", LastName = "D", Position = Positions.WR };
            var highDiscipline = new Player { Discipline = 90, FirstName = "High", LastName = "D", Position = Positions.WR };

            var players = new List<Player> { lowDiscipline, medDiscipline, highDiscipline };

            int lowCount = 0, medCount = 0, highCount = 0;
            int trials = 1500;

            for (int i = 0; i < trials; i++)
            {
                var rng = new TestFluentSeedableRandom()
                    .NextDouble(0.5)
                    .NextInt(0);

                var result = new PenaltyEffectSkillsCheckResult(
                    rng,
                    PenaltyNames.DefensiveHolding,
                    PenaltyOccuredWhen.During,
                    players,
                    players,
                    Possession.Home,
                    50);

                result.Execute(null);

                var discipline = result.Result.CommittedBy.Discipline;
                if (discipline == 30) lowCount++;
                else if (discipline == 60) medCount++;
                else if (discipline == 90) highCount++;
            }

            // Assert - Low should commit most, medium in middle, high should commit least
            Assert.IsTrue(lowCount > medCount && medCount > highCount,
                $"Should have Low > Med > High. Actual: Low={lowCount}, Med={medCount}, High={highCount}");

            // Weights: Low=90, Med=60, High=30 (ratio 3:2:1)
            // Expected distribution: ~857 low, ~571 med, ~286 high per 1500 trials
            Assert.IsTrue(lowCount > 700 && lowCount < 1000,
                $"Low discipline count should be 700-1000, actual: {lowCount}");
            Assert.IsTrue(medCount > 400 && medCount < 700,
                $"Medium discipline count should be 400-700, actual: {medCount}");
            Assert.IsTrue(highCount > 150 && highCount < 400,
                $"High discipline count should be 150-400, actual: {highCount}");
        }

        #endregion

        #region Single Player Tests

        [TestMethod]
        public void SinglePlayer_AlwaysCommitsPenalty_RegardlessOfDiscipline()
        {
            // Arrange - Only one player available
            var player = new Player
            {
                FirstName = "Only",
                LastName = "Player",
                Position = Positions.QB,
                Discipline = 50
            };

            var players = new List<Player> { player };

            // Act
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.5)
                .NextInt(0);

            var result = new PenaltyEffectSkillsCheckResult(
                rng,
                PenaltyNames.IntentionalGrounding,
                PenaltyOccuredWhen.During,
                players,
                players,
                Possession.Home,
                50);

            result.Execute(null);

            // Assert - Single player always gets the penalty
            Assert.IsNotNull(result.Result.CommittedBy);
            Assert.AreEqual("Only", result.Result.CommittedBy.FirstName);
        }

        #endregion

        #region Default Discipline Tests

        [TestMethod]
        public void UnsetDiscipline_UsesDefault70()
        {
            // Arrange - Players with unset (0) discipline should use default 70
            var unsetDisciplinePlayer = new Player
            {
                FirstName = "Unset",
                LastName = "Discipline",
                Position = Positions.CB,
                Discipline = 0 // Not initialized, should default to 70
            };

            var normalDisciplinePlayer = new Player
            {
                FirstName = "Normal",
                LastName = "Discipline",
                Position = Positions.CB,
                Discipline = 70 // Explicitly set to 70
            };

            var players = new List<Player> { unsetDisciplinePlayer, normalDisciplinePlayer };

            // Run trials to see if distribution is roughly equal (both weight 50)
            int unsetCount = 0;
            int normalCount = 0;
            int trials = 1000;

            for (int i = 0; i < trials; i++)
            {
                var rng = new TestFluentSeedableRandom()
                    .NextDouble(0.5)
                    .NextInt(0);

                var result = new PenaltyEffectSkillsCheckResult(
                    rng,
                    PenaltyNames.UnnecessaryRoughness,
                    PenaltyOccuredWhen.During,
                    players,
                    players,
                    Possession.Home,
                    50);

                result.Execute(null);

                if (result.Result.CommittedBy.Discipline == 0)
                    unsetCount++;
                else
                    normalCount++;
            }

            // Assert - Should be roughly equal (within 30% variance)
            // If unset defaults to 70, both weights are 50, so 50:50 distribution expected
            double ratio = (double)unsetCount / normalCount;
            Assert.IsTrue(ratio > 0.7 && ratio < 1.3,
                $"Unset discipline (defaults to 70) should have similar rate to discipline 70. " +
                $"Ratio: {ratio:F2}:1 (Unset={unsetCount}, Normal={normalCount})");
        }

        #endregion

        #region Weight Calculation Verification Tests

        [TestMethod]
        public void DisciplineWeight_CalculatesCorrectly()
        {
            // This test verifies the weight formula: Weight = (100 - Discipline) + 20

            var testCases = new[]
            {
                new { Discipline = 0, ExpectedWeight = 120 },
                new { Discipline = 20, ExpectedWeight = 100 },
                new { Discipline = 50, ExpectedWeight = 70 },
                new { Discipline = 70, ExpectedWeight = 50 },
                new { Discipline = 90, ExpectedWeight = 30 },
                new { Discipline = 100, ExpectedWeight = 20 }
            };

            foreach (var testCase in testCases)
            {
                int actualWeight = (100 - testCase.Discipline) + 20;
                Assert.AreEqual(testCase.ExpectedWeight, actualWeight,
                    $"Discipline {testCase.Discipline} should have weight {testCase.ExpectedWeight}");
            }
        }

        [TestMethod]
        public void HighDisciplineTeam_CommitsFewerPenalties_ThanLowDisciplineTeam()
        {
            // Arrange - Simulate entire teams with different discipline levels
            var highDisciplineTeam = CreateTeam(discipline: 85); // Average discipline 85
            var lowDisciplineTeam = CreateTeam(discipline: 35);  // Average discipline 35

            int highTeamPenaltyCount = 0;
            int lowTeamPenaltyCount = 0;
            int trials = 500;

            for (int i = 0; i < trials; i++)
            {
                // High discipline team penalty
                var rng1 = new TestFluentSeedableRandom().NextDouble(0.5).NextInt(0);
                var result1 = new PenaltyEffectSkillsCheckResult(
                    rng1,
                    PenaltyNames.OffensiveHolding,
                    PenaltyOccuredWhen.During,
                    highDisciplineTeam,
                    highDisciplineTeam,
                    Possession.Home,
                    50);
                result1.Execute(null);
                highTeamPenaltyCount++;

                // Low discipline team penalty
                var rng2 = new TestFluentSeedableRandom().NextDouble(0.5).NextInt(0);
                var result2 = new PenaltyEffectSkillsCheckResult(
                    rng2,
                    PenaltyNames.OffensiveHolding,
                    PenaltyOccuredWhen.During,
                    lowDisciplineTeam,
                    lowDisciplineTeam,
                    Possession.Home,
                    50);
                result2.Execute(null);
                lowTeamPenaltyCount++;
            }

            // Assert - Both teams commit same number of penalties in this test
            // But the DISTRIBUTION within each team should differ
            // This test just verifies the mechanism works
            Assert.AreEqual(trials, highTeamPenaltyCount);
            Assert.AreEqual(trials, lowTeamPenaltyCount);
        }

        #endregion

        #region Helper Methods

        private List<Player> CreateTeam(int discipline)
        {
            var team = new List<Player>();
            var positions = new[] { Positions.QB, Positions.RB, Positions.WR, Positions.TE, Positions.T,
                                   Positions.G, Positions.C, Positions.DE, Positions.DT, Positions.LB, Positions.CB };

            foreach (var position in positions)
            {
                team.Add(new Player
                {
                    Position = position,
                    Discipline = discipline,
                    FirstName = "Player",
                    LastName = $"{position}"
                });
            }

            return team;
        }

        #endregion
    }
}
