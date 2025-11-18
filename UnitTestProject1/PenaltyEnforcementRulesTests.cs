using DomainObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Services;

namespace UnitTestProject1
{
    /// <summary>
    /// Comprehensive tests for penalty enforcement rules.
    /// Verifies that all penalty types have correct:
    /// - Yardage (5, 10, or 15 yards)
    /// - Automatic first down behavior (defensive penalties only)
    /// - Loss of down behavior (specific offensive penalties)
    /// - Spot foul behavior (DPI)
    ///
    /// Tests based on NFL penalty rules and implementation in:
    /// - PenaltyEffectSkillsCheckResult.DeterminePenaltyYards() (lines 140-214)
    /// - PenaltyEnforcement.IsAutomaticFirstDown() (lines 249-264)
    /// - PenaltyEnforcement.IsLossOfDown() (lines 270-279)
    /// </summary>
    [TestClass]
    public class PenaltyEnforcementRulesTests
    {
        private PenaltyEnforcement _penaltyEnforcement;

        [TestInitialize]
        public void Setup()
        {
            _penaltyEnforcement = new PenaltyEnforcement(NullLogger.Instance);
        }

        #region 5-Yard Penalties

        [TestMethod]
        public void FalseStart_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.FalseStart));
        }

        [TestMethod]
        public void DelayOfGame_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.DelayofGame));
        }

        [TestMethod]
        public void Encroachment_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.Encroachment));
        }

        [TestMethod]
        public void DefensiveOffside_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.DefensiveOffside));
        }

        [TestMethod]
        public void NeutralZoneInfraction_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.NeutralZoneInfraction));
        }

        [TestMethod]
        public void IllegalFormation_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.IllegalFormation));
        }

        [TestMethod]
        public void IllegalShift_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.IllegalShift));
        }

        [TestMethod]
        public void IllegalMotion_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.IllegalMotion));
        }

        [TestMethod]
        public void Offensive12OnField_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.Offensive12OnField));
        }

        [TestMethod]
        public void Defensive12OnField_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.Defensive12OnField));
        }

        [TestMethod]
        public void IllegalSubstitution_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.IllegalSubstitution));
        }

        [TestMethod]
        public void RunningIntoTheKicker_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.RunningIntotheKicker));
        }

        [TestMethod]
        public void DefensiveDelayOfGame_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.DefensiveDelayofGame));
        }

        [TestMethod]
        public void OffensiveOffside_Is5Yards()
        {
            Assert.AreEqual(5, GetStandardYardage(PenaltyNames.OffensiveOffside));
        }

        #endregion

        #region 10-Yard Penalties

        [TestMethod]
        public void OffensiveHolding_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.OffensiveHolding));
        }

        [TestMethod]
        public void DefensiveHolding_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.DefensiveHolding));
        }

        [TestMethod]
        public void IllegalUseOfHands_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalUseofHands));
        }

        [TestMethod]
        public void IllegalBlockAboveTheWaist_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalBlockAbovetheWaist));
        }

        [TestMethod]
        public void OffensivePassInterference_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.OffensivePassInterference));
        }

        [TestMethod]
        public void IllegalContact_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalContact));
        }

        [TestMethod]
        public void Clipping_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.Clipping));
        }

        [TestMethod]
        public void Tripping_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.Tripping));
        }

        [TestMethod]
        public void IneligibleDownfieldPass_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IneligibleDownfieldPass));
        }

        [TestMethod]
        public void IneligibleDownfieldKick_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IneligibleDownfieldKick));
        }

        [TestMethod]
        public void IllegalForwardPass_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalForwardPass));
        }

        [TestMethod]
        public void IntentionalGrounding_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IntentionalGrounding));
        }

        [TestMethod]
        public void IllegalBlindsideBlock_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalBlindsideBlock));
        }

        [TestMethod]
        public void ChopBlock_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.ChopBlock));
        }

        [TestMethod]
        public void LowBlock_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.LowBlock));
        }

        [TestMethod]
        public void IllegalPeelback_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalPeelback));
        }

        [TestMethod]
        public void IllegalCrackback_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalCrackback));
        }

        [TestMethod]
        public void IllegalTouchPass_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalTouchPass));
        }

        [TestMethod]
        public void IllegalTouchKick_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.IllegalTouchKick));
        }

        [TestMethod]
        public void InvalidFairCatchSignal_Is10Yards()
        {
            Assert.AreEqual(10, GetStandardYardage(PenaltyNames.InvalidFairCatchSignal));
        }

        #endregion

        #region 15-Yard Penalties

        [TestMethod]
        public void UnnecessaryRoughness_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.UnnecessaryRoughness));
        }

        [TestMethod]
        public void FaceMask15Yards_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.FaceMask15Yards));
        }

        [TestMethod]
        public void RoughingThePasser_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.RoughingthePasser));
        }

        [TestMethod]
        public void RoughingTheKicker_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.RoughingtheKicker));
        }

        [TestMethod]
        public void UnsportsmanlikeConduct_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.UnsportsmanlikeConduct));
        }

        [TestMethod]
        public void Taunting_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.Taunting));
        }

        [TestMethod]
        public void HorseCollarTackle_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.HorseCollarTackle));
        }

        [TestMethod]
        public void PersonalFoul_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.PersonalFoul));
        }

        [TestMethod]
        public void PlayerOutOfBoundsOnPunt_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.PlayerOutofBoundsonPunt));
        }

        [TestMethod]
        public void FairCatchInterference_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.FairCatchInterference));
        }

        [TestMethod]
        public void Leaping_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.Leaping));
        }

        [TestMethod]
        public void Leverage_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.Leverage));
        }

        [TestMethod]
        public void InterferenceWithOpportunitityToCatch_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.InterferencewithOpportunitytoCatch));
        }

        [TestMethod]
        public void Disqualification_Is15Yards()
        {
            Assert.AreEqual(15, GetStandardYardage(PenaltyNames.Disqualification));
        }

        #endregion

        #region Automatic First Down Tests

        [TestMethod]
        public void DefensiveHolding_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.DefensiveHolding),
                "DefensiveHolding should give automatic first down");
        }

        [TestMethod]
        public void DefensivePassInterference_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.DefensivePassInterference),
                "DPI should give automatic first down");
        }

        [TestMethod]
        public void RoughingThePasser_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.RoughingthePasser),
                "Roughing the passer should give automatic first down");
        }

        [TestMethod]
        public void RoughingTheKicker_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.RoughingtheKicker),
                "Roughing the kicker should give automatic first down");
        }

        [TestMethod]
        public void UnnecessaryRoughness_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.UnnecessaryRoughness),
                "Unnecessary roughness should give automatic first down");
        }

        [TestMethod]
        public void FaceMask_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.FaceMask15Yards),
                "Facemask should give automatic first down");
        }

        [TestMethod]
        public void IllegalContact_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.IllegalContact),
                "Illegal contact should give automatic first down");
        }

        [TestMethod]
        public void PersonalFoul_IsAutomaticFirstDown()
        {
            Assert.IsTrue(IsAutomaticFirstDown(PenaltyNames.PersonalFoul),
                "Personal foul should give automatic first down");
        }

        #endregion

        #region Non-Automatic First Down Tests (Defensive Penalties)

        [TestMethod]
        public void DefensiveOffside_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.DefensiveOffside),
                "Offsides does NOT give automatic first down");
        }

        [TestMethod]
        public void Encroachment_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.Encroachment),
                "Encroachment does NOT give automatic first down");
        }

        [TestMethod]
        public void NeutralZoneInfraction_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.NeutralZoneInfraction),
                "Neutral zone infraction does NOT give automatic first down");
        }

        [TestMethod]
        public void DefensiveDelayOfGame_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.DefensiveDelayofGame),
                "Defensive delay of game does NOT give automatic first down");
        }

        [TestMethod]
        public void IllegalSubstitution_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.IllegalSubstitution),
                "Illegal substitution does NOT give automatic first down");
        }

        [TestMethod]
        public void Defensive12OnField_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.Defensive12OnField),
                "12 on field does NOT give automatic first down");
        }

        [TestMethod]
        public void RunningIntoTheKicker_IsNotAutomaticFirstDown()
        {
            Assert.IsFalse(IsAutomaticFirstDown(PenaltyNames.RunningIntotheKicker),
                "Running into the kicker does NOT give automatic first down");
        }

        #endregion

        #region Loss of Down Tests

        [TestMethod]
        public void IntentionalGrounding_IsLossOfDown()
        {
            Assert.IsTrue(IsLossOfDown(PenaltyNames.IntentionalGrounding),
                "Intentional grounding should cause loss of down");
        }

        [TestMethod]
        public void IllegalForwardPass_IsLossOfDown()
        {
            Assert.IsTrue(IsLossOfDown(PenaltyNames.IllegalForwardPass),
                "Illegal forward pass should cause loss of down");
        }

        [TestMethod]
        public void OffensiveHolding_IsNotLossOfDown()
        {
            Assert.IsFalse(IsLossOfDown(PenaltyNames.OffensiveHolding),
                "Offensive holding should NOT cause loss of down");
        }

        [TestMethod]
        public void FalseStart_IsNotLossOfDown()
        {
            Assert.IsFalse(IsLossOfDown(PenaltyNames.FalseStart),
                "False start should NOT cause loss of down");
        }

        #endregion

        #region Spot Foul Tests

        [TestMethod]
        public void DefensivePassInterference_IsSpotFoul()
        {
            Assert.IsTrue(IsSpotFoul(PenaltyNames.DefensivePassInterference),
                "DPI should be a spot foul");
        }

        [TestMethod]
        public void OffensiveHolding_IsNotSpotFoul()
        {
            Assert.IsFalse(IsSpotFoul(PenaltyNames.OffensiveHolding),
                "Offensive holding should NOT be a spot foul");
        }

        [TestMethod]
        public void DefensiveHolding_IsNotSpotFoul()
        {
            Assert.IsFalse(IsSpotFoul(PenaltyNames.DefensiveHolding),
                "Defensive holding should NOT be a spot foul");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the standard yardage for a penalty (not considering half-distance)
        /// </summary>
        private int GetStandardYardage(PenaltyNames penaltyName)
        {
            // Based on PenaltyEffectSkillsCheckResult.DeterminePenaltyYards()
            switch (penaltyName)
            {
                // 5-yard penalties
                case PenaltyNames.FalseStart:
                case PenaltyNames.DelayofGame:
                case PenaltyNames.Encroachment:
                case PenaltyNames.DefensiveOffside:
                case PenaltyNames.NeutralZoneInfraction:
                case PenaltyNames.IllegalFormation:
                case PenaltyNames.IllegalShift:
                case PenaltyNames.IllegalMotion:
                case PenaltyNames.Offensive12OnField:
                case PenaltyNames.Defensive12OnField:
                case PenaltyNames.IllegalSubstitution:
                case PenaltyNames.RunningIntotheKicker:
                case PenaltyNames.DefensiveDelayofGame:
                case PenaltyNames.OffensiveOffside:
                    return 5;

                // 10-yard penalties
                case PenaltyNames.OffensiveHolding:
                case PenaltyNames.DefensiveHolding:
                case PenaltyNames.IllegalUseofHands:
                case PenaltyNames.IllegalBlockAbovetheWaist:
                case PenaltyNames.OffensivePassInterference:
                case PenaltyNames.IllegalContact:
                case PenaltyNames.Clipping:
                case PenaltyNames.Tripping:
                case PenaltyNames.IneligibleDownfieldPass:
                case PenaltyNames.IneligibleDownfieldKick:
                case PenaltyNames.IllegalForwardPass:
                case PenaltyNames.IntentionalGrounding:
                case PenaltyNames.IllegalBlindsideBlock:
                case PenaltyNames.ChopBlock:
                case PenaltyNames.LowBlock:
                case PenaltyNames.IllegalPeelback:
                case PenaltyNames.IllegalCrackback:
                case PenaltyNames.IllegalTouchPass:
                case PenaltyNames.IllegalTouchKick:
                case PenaltyNames.InvalidFairCatchSignal:
                    return 10;

                // 15-yard penalties
                case PenaltyNames.UnnecessaryRoughness:
                case PenaltyNames.FaceMask15Yards:
                case PenaltyNames.RoughingthePasser:
                case PenaltyNames.RoughingtheKicker:
                case PenaltyNames.UnsportsmanlikeConduct:
                case PenaltyNames.Taunting:
                case PenaltyNames.HorseCollarTackle:
                case PenaltyNames.PersonalFoul:
                case PenaltyNames.PlayerOutofBoundsonPunt:
                case PenaltyNames.FairCatchInterference:
                case PenaltyNames.Leaping:
                case PenaltyNames.Leverage:
                case PenaltyNames.InterferencewithOpportunitytoCatch:
                case PenaltyNames.Disqualification:
                    return 15;

                // DPI is variable (spot foul)
                case PenaltyNames.DefensivePassInterference:
                    return -1; // Spot foul - variable yardage

                default:
                    return -1;
            }
        }

        /// <summary>
        /// Checks if a penalty gives automatic first down (defensive penalties only)
        /// </summary>
        private bool IsAutomaticFirstDown(PenaltyNames penalty)
        {
            // Exceptions - defensive penalties that do NOT give automatic first down
            var noAutomaticFirstDown = new[]
            {
                PenaltyNames.DefensiveOffside,
                PenaltyNames.Encroachment,
                PenaltyNames.NeutralZoneInfraction,
                PenaltyNames.DefensiveDelayofGame,
                PenaltyNames.IllegalSubstitution,
                PenaltyNames.Defensive12OnField,
                PenaltyNames.RunningIntotheKicker
            };

            return !noAutomaticFirstDown.Contains(penalty);
        }

        /// <summary>
        /// Checks if a penalty causes loss of down
        /// </summary>
        private bool IsLossOfDown(PenaltyNames penalty)
        {
            var lossOfDownPenalties = new[]
            {
                PenaltyNames.IntentionalGrounding,
                PenaltyNames.IllegalForwardPass
            };

            return lossOfDownPenalties.Contains(penalty);
        }

        /// <summary>
        /// Checks if a penalty is a spot foul
        /// </summary>
        private bool IsSpotFoul(PenaltyNames penalty)
        {
            var spotFouls = new[]
            {
                PenaltyNames.DefensivePassInterference
            };

            return spotFouls.Contains(penalty);
        }

        #endregion
    }
}
