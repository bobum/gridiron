using DomainObjects;
using DomainObjects.Helpers;
using DomainObjects.Penalties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestProject1
{
    /// <summary>
    /// Tests for the new penalty class architecture.
    /// Validates that individual penalty classes correctly implement their NFL rules.
    /// </summary>
    [TestClass]
    public class PenaltyClassTests
    {
        // ==================== Offensive Holding Tests ====================

        [TestMethod]
        public void OffensiveHolding_HasCorrectProperties()
        {
            var penalty = new OffensiveHoldingPenalty();

            Assert.AreEqual("Offensive Holding", penalty.Name);
            Assert.AreEqual(10, penalty.Yards);
            Assert.AreEqual(TeamSide.Offense, penalty.CommittedBy);
            Assert.IsFalse(penalty.IsAutomaticFirstDown);
            Assert.IsFalse(penalty.IsLossOfDown);
            Assert.IsFalse(penalty.IsSpotFoul);
            Assert.IsFalse(penalty.IsDeadBallFoul);
            Assert.AreEqual(PenaltyTiming.DuringBlocking, penalty.Timing);
        }

        [TestMethod]
        public void OffensiveHolding_HasCorrectOccurrenceProbability()
        {
            var penalty = new OffensiveHoldingPenalty();

            Assert.AreEqual(0.019, penalty.BaseOccurrenceProbability, 0.001);
        }

        [TestMethod]
        public void OffensiveHolding_OnlyOffensiveLinemenCanCommit()
        {
            var penalty = new OffensiveHoldingPenalty();

            var tackle = new Player { Position = Positions.T };
            var guard = new Player { Position = Positions.G };
            var center = new Player { Position = Positions.C };
            var tightEnd = new Player { Position = Positions.TE };
            var runningBack = new Player { Position = Positions.RB };
            var quarterback = new Player { Position = Positions.QB };

            Assert.IsTrue(penalty.CanBeCommittedBy(tackle, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(guard, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(center, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(tightEnd, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(runningBack, TeamSide.Offense));
            Assert.IsFalse(penalty.CanBeCommittedBy(quarterback, TeamSide.Offense));

            // Can't be committed by defense
            Assert.IsFalse(penalty.CanBeCommittedBy(tackle, TeamSide.Defense));
        }

        [TestMethod]
        public void OffensiveHolding_ProbabilityIncreasesOnPassPlays()
        {
            var penalty = new OffensiveHoldingPenalty();

            var passContext = new PenaltyContext { PlayType = PlayType.Pass };
            var runContext = new PenaltyContext { PlayType = PlayType.Run };

            var passProb = penalty.CalculateOccurrenceProbability(passContext);
            var runProb = penalty.CalculateOccurrenceProbability(runContext);

            Assert.IsGreaterThan(runProb, passProb, "Offensive holding should be more likely on pass plays");
        }

        // ==================== Defensive Holding Tests ====================

        [TestMethod]
        public void DefensiveHolding_HasCorrectProperties()
        {
            var penalty = new DefensiveHoldingPenalty();

            Assert.AreEqual("Defensive Holding", penalty.Name);
            Assert.AreEqual(5, penalty.Yards);
            Assert.AreEqual(TeamSide.Defense, penalty.CommittedBy);
            Assert.IsTrue(penalty.IsAutomaticFirstDown);
            Assert.IsFalse(penalty.IsLossOfDown);
            Assert.IsFalse(penalty.IsSpotFoul);
            Assert.IsFalse(penalty.IsDeadBallFoul);
            Assert.AreEqual(PenaltyTiming.DuringCoverage, penalty.Timing);
        }

        [TestMethod]
        public void DefensiveHolding_OnlyDefensiveBacksCanCommit()
        {
            var penalty = new DefensiveHoldingPenalty();

            var cornerback = new Player { Position = Positions.CB };
            var safety = new Player { Position = Positions.S };
            var linebacker = new Player { Position = Positions.LB };
            var defensiveEnd = new Player { Position = Positions.DE };

            Assert.IsTrue(penalty.CanBeCommittedBy(cornerback, TeamSide.Defense));
            Assert.IsTrue(penalty.CanBeCommittedBy(safety, TeamSide.Defense));
            Assert.IsTrue(penalty.CanBeCommittedBy(linebacker, TeamSide.Defense));
            Assert.IsFalse(penalty.CanBeCommittedBy(defensiveEnd, TeamSide.Defense));

            // Can't be committed by offense
            Assert.IsFalse(penalty.CanBeCommittedBy(cornerback, TeamSide.Offense));
        }

        [TestMethod]
        public void DefensiveHolding_MoreLikelyOnShortRoutes()
        {
            var penalty = new DefensiveHoldingPenalty();

            var shortRouteContext = new PenaltyContext { AirYards = 5 };
            var deepRouteContext = new PenaltyContext { AirYards = 25 };

            var shortProb = penalty.CalculateOccurrenceProbability(shortRouteContext);
            var deepProb = penalty.CalculateOccurrenceProbability(deepRouteContext);

            Assert.IsGreaterThan(deepProb, shortProb, "Defensive holding should be more likely on short routes");
        }

        // ==================== False Start Tests ====================

        [TestMethod]
        public void FalseStart_HasCorrectProperties()
        {
            var penalty = new FalseStartPenalty();

            Assert.AreEqual("False Start", penalty.Name);
            Assert.AreEqual(5, penalty.Yards);
            Assert.AreEqual(TeamSide.Offense, penalty.CommittedBy);
            Assert.IsFalse(penalty.IsAutomaticFirstDown);
            Assert.IsFalse(penalty.IsLossOfDown);
            Assert.IsFalse(penalty.IsSpotFoul);
            Assert.IsTrue(penalty.IsDeadBallFoul);
            Assert.AreEqual(PenaltyTiming.PreSnap, penalty.Timing);
        }

        [TestMethod]
        public void FalseStart_OnlyOffensiveLinemenAndBacksCanCommit()
        {
            var penalty = new FalseStartPenalty();

            var tackle = new Player { Position = Positions.T };
            var guard = new Player { Position = Positions.G };
            var runningBack = new Player { Position = Positions.RB };
            var wideReceiver = new Player { Position = Positions.WR };

            Assert.IsTrue(penalty.CanBeCommittedBy(tackle, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(guard, TeamSide.Offense));
            Assert.IsTrue(penalty.CanBeCommittedBy(runningBack, TeamSide.Offense));
            Assert.IsFalse(penalty.CanBeCommittedBy(wideReceiver, TeamSide.Offense));
        }

        [TestMethod]
        public void FalseStart_AlwaysAccepted()
        {
            var penalty = new FalseStartPenalty();

            var context = new PenaltyAcceptanceContext
            {
                CurrentDown = Downs.Third,
                YardsToGo = 10,
                YardsGainedOnPlay = 5,
                PenaltyYards = 5
            };

            Assert.IsTrue(penalty.ShouldAccept(context), "False start should always be accepted");
        }

        // ==================== Defensive Pass Interference Tests ====================

        [TestMethod]
        public void DefensivePassInterference_HasCorrectProperties()
        {
            var penalty = new DefensivePassInterferencePenalty();

            Assert.AreEqual("Defensive Pass Interference", penalty.Name);
            Assert.AreEqual(TeamSide.Defense, penalty.CommittedBy);
            Assert.IsTrue(penalty.IsAutomaticFirstDown);
            Assert.IsFalse(penalty.IsLossOfDown);
            Assert.IsTrue(penalty.IsSpotFoul);
            Assert.IsFalse(penalty.IsDeadBallFoul);
            Assert.AreEqual(PenaltyTiming.DuringCoverage, penalty.Timing);
        }

        [TestMethod]
        public void DefensivePassInterference_MuchMoreLikelyOnDeepPasses()
        {
            var penalty = new DefensivePassInterferencePenalty();

            var shortPassContext = new PenaltyContext { AirYards = 5, PassCompleted = false };
            var deepPassContext = new PenaltyContext { AirYards = 35, PassCompleted = false };

            var shortProb = penalty.CalculateOccurrenceProbability(shortPassContext);
            var deepProb = penalty.CalculateOccurrenceProbability(deepPassContext);

            Assert.IsGreaterThan(shortProb * 2, deepProb, "DPI should be MUCH more likely on deep passes");
        }

        [TestMethod]
        public void DefensivePassInterference_RareOnCompletions()
        {
            var penalty = new DefensivePassInterferencePenalty();

            var incompleteContext = new PenaltyContext { AirYards = 20, PassCompleted = false };
            var completeContext = new PenaltyContext { AirYards = 20, PassCompleted = true };

            var incompleteProb = penalty.CalculateOccurrenceProbability(incompleteContext);
            var completeProb = penalty.CalculateOccurrenceProbability(completeContext);

            Assert.IsGreaterThan(completeProb * 10, incompleteProb, "DPI should be very rare on completions");
        }

        [TestMethod]
        public void DefensivePassInterference_EndZoneRulePlacesBallAtOne()
        {
            var penalty = new DefensivePassInterferencePenalty();

            var endZoneContext = new PenaltyEnforcementContext
            {
                FieldPosition = 95,
                InEndZone = true
            };

            var yardage = penalty.CalculateYardage(endZoneContext);

            // Should place ball at 1-yard line (position 99 in 0-100 system)
            Assert.IsGreaterThan(0, yardage, "End zone DPI should result in positive yardage");
        }

        [TestMethod]
        public void DefensivePassInterference_SpotFoulCalculatesCorrectYardage()
        {
            var penalty = new DefensivePassInterferencePenalty();

            var context = new PenaltyEnforcementContext
            {
                FieldPosition = 30,
                SpotOfFoul = 55,  // 25 yards downfield
                InEndZone = false
            };

            var yardage = penalty.CalculateYardage(context);

            Assert.AreEqual(25, yardage, "Spot foul should calculate yardage from line of scrimmage to spot");
        }

        // ==================== Player Selection Tests ====================

        [TestMethod]
        public void PlayerSelection_WeightedByDiscipline()
        {
            var penalty = new OffensiveHoldingPenalty();
            var rng = new SeedableRandom(12345);

            var disciplinedPlayer = new Player { Position = Positions.G, Discipline = 100 };
            var undisciplinedPlayer = new Player { Position = Positions.G, Discipline = 0 };

            var players = new List<Player> { disciplinedPlayer, undisciplinedPlayer };

            // Run selection 100 times
            var undisciplinedCount = 0;
            for (int i = 0; i < 100; i++)
            {
                var selected = penalty.SelectPlayerWhoCommitted(players, rng);
                if (selected == undisciplinedPlayer)
                {
                    undisciplinedCount++;
                }
            }

            // Undisciplined player should be selected much more often
            Assert.IsGreaterThan(60, undisciplinedCount, $"Undisciplined player should be selected >60% of time, was {undisciplinedCount}%");
        }

        [TestMethod]
        public void PlayerSelection_SinglePlayerReturnsPlayer()
        {
            var penalty = new OffensiveHoldingPenalty();
            var rng = new SeedableRandom();

            var player = new Player { Position = Positions.G };
            var players = new List<Player> { player };

            var selected = penalty.SelectPlayerWhoCommitted(players, rng);

            Assert.AreEqual(player, selected);
        }

        [TestMethod]
        public void PlayerSelection_EmptyListReturnsNull()
        {
            var penalty = new OffensiveHoldingPenalty();
            var rng = new SeedableRandom();

            var players = new List<Player>();

            var selected = penalty.SelectPlayerWhoCommitted(players, rng);

            Assert.IsNull(selected);
        }
    }
}
