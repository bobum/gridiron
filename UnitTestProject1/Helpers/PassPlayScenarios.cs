using System;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for pass play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for pass plays. Pass plays have multiple execution paths
    /// (sack, completion with YAC, completion with immediate tackle, incompletion, interception), each with
    /// different random value sequences.
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    ///
    /// SACK PATH:
    /// 1. Pass protection check (FAILS >= ~0.75)
    /// 2. Blocking penalty check
    /// 3. Sack yards (NextInt 2-10)
    /// 4. Roughing the passer penalty check
    /// 5. Fumble check
    /// 6. Elapsed time factor
    ///
    /// COMPLETION WITH IMMEDIATE TACKLE PATH (YAC fails):
    /// 1. Pass protection check (succeeds)
    /// 2. Blocking penalty check
    /// 3. QB pressure check
    /// 4. Receiver selection
    /// 5. Pass type determination
    /// 6. Air yards
    /// 7. Pass completion check (succeeds)
    /// 8. Receiver tackle penalty check
    /// 9. YAC opportunity check (FAILS)
    /// 10. YAC yards (NextInt 0-2 for immediate tackle)
    /// 11. Fumble check
    /// 12. Elapsed time factor
    ///
    /// COMPLETION WITH YAC PATH (YAC succeeds):
    /// 1-8. Same as immediate tackle
    /// 9. YAC opportunity check (SUCCEEDS)
    /// 10. YAC random factor
    /// 11. Big play check
    /// 12. [Conditional] Big play bonus yards (NextInt 10-30) - only if big play succeeds
    /// 13. Receiver tackle penalty check
    /// 14. Fumble check
    /// 15. Elapsed time factor
    ///
    /// INCOMPLETE PASS PATH:
    /// 1-7. Same as completion path
    /// 8. Pass completion check (FAILS)
    /// 9. Coverage penalty check (only on incomplete!)
    /// 10. Interception occurred check
    /// 11. Elapsed time factor
    ///
    /// INTERCEPTION PATH:
    /// 1-9. Same as incomplete
    /// 10. Interception occurred check (SUCCEEDS)
    /// 11. Interception return base factor
    /// 12. Interception return variance factor
    /// 13. Tackle penalty check (for return)
    /// 14. Fumble check
    /// 15. Elapsed time factor
    /// </summary>
    public static class PassPlayScenarios
    {
        #region Completed Pass Scenarios

        /// <summary>
        /// Completed pass with immediate tackle - receiver caught and tackled right away.
        /// Most common scenario for short/medium completions.
        ///
        /// Random sequence: Protection → Pressure → Receiver → Type → Air yards → Completion →
        /// Receiver tackle penalty → YAC fails → Immediate tackle yards → Fumble → Elapsed time
        /// </summary>
        /// <param name="airYards">Yards ball travels in air</param>
        /// <param name="immediateTackleYards">Yards after catch when tackled immediately (0-2)</param>
        /// <param name="passType">Pass type value: Screen<0.15, Short 0.15-0.50, Forward 0.50-0.85, Deep>0.85</param>
        /// <param name="pressure">Whether QB is under pressure</param>
        public static TestFluentSeedableRandom CompletedPassImmediateTackle(
            int airYards = 10,
            int immediateTackleYards = 2,
            double passType = 0.6,
            bool pressure = false)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)              // Protection holds
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(pressure ? 0.2 : 0.8) // Pressure or not
                .ReceiverSelection(0.5)                // Weighted receiver selection
                .PassTypeDetermination(passType)       // Pass type (default: Forward)
                .AirYards(airYards)                    // Distance in air
                .PassCompletionCheck(0.5)              // Completion succeeds
                .NextDouble(0.99)                      // No receiver tackle penalty
                .YACOpportunityCheck(0.8)              // YAC fails (tackled immediately)
                .ImmediateTackleYards(immediateTackleYards) // 0-2 yards after catch
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);        // ~7 seconds
        }

        /// <summary>
        /// Completed pass with good YAC - receiver breaks tackles for extra yards.
        ///
        /// Random sequence: Same as immediate tackle but YAC succeeds, adds YACRandomFactor and BigPlayCheck.
        /// NOTE: Adds 1-2 extra random values compared to immediate tackle (YACRandomFactor always, BigPlayBonusYards conditionally).
        /// </summary>
        /// <param name="airYards">Yards ball travels in air</param>
        /// <param name="yacFactor">YAC random factor (0-1), formula: factor * 8 - 2 = -2 to +6 yards</param>
        /// <param name="hasBigPlay">Whether receiver breaks for big play (requires Speed > 85)</param>
        /// <param name="bigPlayYards">Bonus yards if big play (10-30)</param>
        /// <param name="passType">Pass type value: Screen<0.15, Short 0.15-0.50, Forward 0.50-0.85, Deep>0.85</param>
        /// <param name="receiverSelection">Receiver selection value (0-1), higher values favor better receivers</param>
        /// <param name="protectionValue">Pass protection check value (default 0.7)</param>
        public static TestFluentSeedableRandom CompletedPassWithYAC(
            int airYards = 10,
            double yacFactor = 0.5,
            bool hasBigPlay = false,
            int bigPlayYards = 20,
            double passType = 0.6,
            double receiverSelection = 0.5,
            double protectionValue = 0.7)
        {
            var rng = new TestFluentSeedableRandom()
                .PassProtectionCheck(protectionValue)  // Protection holds (customizable)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.8)                  // No pressure
                .ReceiverSelection(receiverSelection)  // Weighted receiver selection (customizable)
                .PassTypeDetermination(passType)       // Pass type (customizable)
                .AirYards(airYards)                    // Distance in air
                .PassCompletionCheck(0.5)              // Completion succeeds
                .YACOpportunityCheck(0.3)              // YAC succeeds!
                .YACRandomFactor(yacFactor)            // YAC variance
                .BigPlayCheck(hasBigPlay ? 0.02 : 0.9); // Big play check

            if (hasBigPlay)
                rng.BigPlayBonusYards(bigPlayYards);   // EXTRA: Big play bonus

            rng.NextDouble(0.99)                       // No receiver tackle penalty
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);        // ~7 seconds

            return rng;
        }

        /// <summary>
        /// Screen pass - short pass behind line of scrimmage or just beyond.
        /// Pass type < 0.15, air yards typically -3 to +3.
        /// </summary>
        public static TestFluentSeedableRandom ScreenPass(int airYards = 1, double yacFactor = 0.5)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.10)           // Screen (< 0.15)
                .AirYards(airYards)
                .PassCompletionCheck(0.5)
                .YACOpportunityCheck(0.3)              // Usually good YAC on screens
                .YACRandomFactor(yacFactor)
                .BigPlayCheck(0.9)                     // No big play
                .NextDouble(0.99)                      // No receiver tackle penalty
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        /// <summary>
        /// Short pass - 3-12 yards in air.
        /// Pass type 0.15-0.50.
        /// </summary>
        public static TestFluentSeedableRandom ShortPass(int airYards = 7)
        {
            return CompletedPassImmediateTackle(airYards: airYards, passType: 0.3); // 0.15-0.50 range
        }

        /// <summary>
        /// Deep pass - 18-45 yards in air.
        /// Pass type > 0.85.
        /// </summary>
        public static TestFluentSeedableRandom DeepPass(int airYards = 30, bool withYAC = true)
        {
            if (withYAC)
            {
                return CompletedPassWithYAC(airYards: airYards, yacFactor: 0.5, passType: 0.90); // > 0.85 for Deep
            }
            else
            {
                return CompletedPassImmediateTackle(airYards: airYards, passType: 0.90); // > 0.85
            }
        }

        #endregion

        #region Incomplete Pass Scenarios

        /// <summary>
        /// Incomplete pass - pass falls incomplete, 0 yards gained.
        ///
        /// Random sequence: Protection → Pressure → Receiver → Type → Air yards → Completion fails →
        /// Coverage penalty → Interception check → Elapsed time
        ///
        /// NOTE: Different path from completion - has coverage penalty check instead of YAC checks.
        /// </summary>
        /// <param name="withPressure">Whether QB is under pressure (affects completion %)</param>
        public static TestFluentSeedableRandom IncompletePass(bool withPressure = false)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(withPressure ? 0.8 : 0.3) // More pressure = less protection
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(withPressure ? 0.2 : 0.5) // Pressure affects completion
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)            // Forward pass
                .AirYards(8)
                .PassCompletionCheck(0.9)              // FAILS (incomplete)
                .NextDouble(0.99)                      // No coverage penalty
                .InterceptionOccurredCheck(0.99)       // No interception
                .ElapsedTimeRandomFactor(0.99);
        }

        #endregion

        #region Sack Scenarios

        /// <summary>
        /// Sack - QB tackled behind line of scrimmage for loss.
        ///
        /// Random sequence: Protection fails → Blocking penalty → Sack yards → Roughing penalty → Fumble → Elapsed time
        ///
        /// NOTE: Much shorter sequence than pass attempts - only 6 random values.
        /// </summary>
        /// <param name="sackYards">Yards lost on sack (2-10, returned as negative)</param>
        /// <param name="withFumble">Whether QB fumbles on sack</param>
        public static TestFluentSeedableRandom Sack(int sackYards = 7, bool withFumble = false)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)              // FAILS - Sack!
                .NextDouble(0.99)                      // No blocking penalty
                .SackYards(sackYards)                  // Sack yardage loss
                .NextDouble(0.99)                      // No roughing the passer
                .NextDouble(withFumble ? 0.01 : 0.99)  // Fumble check
                .ElapsedTimeRandomFactor(0.99);        // ~3 seconds (shorter than pass)
        }

        #endregion

        #region Interception Scenarios

        /// <summary>
        /// Interception - pass intercepted and returned.
        ///
        /// Random sequence: Protection → Pressure → Receiver → Type → Air yards → Completion fails →
        /// Coverage penalty → Interception succeeds → Return base → Return variance → Tackle penalty → Fumble → Elapsed time
        ///
        /// NOTE: Adds return calculations compared to incomplete pass.
        /// </summary>
        /// <param name="returnYards">Approximate return yards (actual calculated from base + variance)</param>
        /// <param name="withFumble">Whether interceptor fumbles on return</param>
        public static TestFluentSeedableRandom Interception(int returnYards = 15, bool withFumble = false)
        {
            // Return calculation: base (8-15) + variance (-5 to +25)
            // To get specific return yards, we need to work backwards
            double returnBase = 0.5;      // 8 + (0.5 * 7) = 11.5
            double returnVariance = 0.5;  // (0.5 * 30) - 5 = 10
            // Total: ~21.5 yards

            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.2)                  // Pressure increases INT chance
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(10)
                .PassCompletionCheck(0.9)              // Incomplete
                .NextDouble(0.99)                      // No coverage penalty
                .InterceptionOccurredCheck(0.01)       // INTERCEPTION!
                .InterceptionReturnBase(returnBase)    // Base return yards
                .InterceptionReturnVariance(returnVariance) // Return variance
                .NextDouble(0.99)                      // No tackle penalty on return
                .NextDouble(withFumble ? 0.01 : 0.99)  // Fumble check
                .ElapsedTimeRandomFactor(0.99);
        }

        /// <summary>
        /// Interception returned for touchdown - long return with no tackle.
        /// </summary>
        public static TestFluentSeedableRandom InterceptionTouchdown()
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.2)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(10)
                .PassCompletionCheck(0.9)
                .NextDouble(0.99)                      // No coverage penalty
                .InterceptionOccurredCheck(0.01)       // INTERCEPTION!
                .InterceptionReturnBase(0.9)           // High base (14.3 yards)
                .InterceptionReturnVariance(0.9)       // High variance (22 yards)
                .NextDouble(0.99)                      // No tackle penalty
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Pass with blocking penalty (offensive holding during pass protection).
        ///
        /// Random sequence: Same as completed pass but blocking penalty check = 0.01 + penalty effect random values.
        /// </summary>
        public static TestFluentSeedableRandom WithBlockingPenalty(int airYards = 10)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .NextDouble(0.01)                      // BLOCKING PENALTY!
                .NextDouble(0.5)                       // Penalty effect: team selection
                .NextInt(5)                            // Penalty effect: player selection
                .QBPressureCheck(0.8)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(airYards)
                .PassCompletionCheck(0.5)
                .NextDouble(0.99)                      // No receiver tackle penalty
                .YACOpportunityCheck(0.8)
                .ImmediateTackleYards(2)
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        /// <summary>
        /// Completed pass with tackle penalty on receiver (unnecessary roughness, etc.).
        ///
        /// Random sequence: Same as completed pass but receiver tackle penalty = 0.01 + penalty effect random values.
        /// </summary>
        public static TestFluentSeedableRandom WithReceiverTacklePenalty(int airYards = 10)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.8)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(airYards)
                .PassCompletionCheck(0.5)
                .NextDouble(0.01)                      // RECEIVER TACKLE PENALTY!
                .NextDouble(0.5)                       // Penalty effect: team selection
                .NextInt(5)                            // Penalty effect: player selection
                .YACOpportunityCheck(0.8)
                .ImmediateTackleYards(2)
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        /// <summary>
        /// Sack with roughing the passer penalty.
        ///
        /// Random sequence: Sack path but roughing penalty = 0.01.
        /// </summary>
        public static TestFluentSeedableRandom WithRoughingThePasser(int sackYards = 7)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.8)              // Sack
                .NextDouble(0.99)                      // No blocking penalty
                .SackYards(sackYards)
                .NextDouble(0.01)                      // ROUGHING THE PASSER!
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        /// <summary>
        /// Incomplete pass with defensive pass interference penalty.
        ///
        /// Random sequence: Incomplete path but coverage penalty = 0.01.
        /// </summary>
        public static TestFluentSeedableRandom WithPassInterference()
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.3)
                .NextDouble(0.99)                      // No blocking penalty
                .QBPressureCheck(0.5)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(20)                          // Deep ball = more likely PI
                .PassCompletionCheck(0.9)              // Incomplete
                .NextDouble(0.01)                      // PASS INTERFERENCE!
                .InterceptionOccurredCheck(0.99)       // No interception
                .ElapsedTimeRandomFactor(0.99);
        }

        #endregion

        #region Multiple Penalty Scenarios

        /// <summary>
        /// Completed pass with BOTH blocking penalty AND receiver tackle penalty.
        /// Tests that multiple penalties can be detected on the same play.
        ///
        /// Random sequence: Protection → BLOCKING PENALTY → Penalty effect (team + player) →
        /// Pressure → Receiver → Type → Air yards → Completion → RECEIVER TACKLE PENALTY →
        /// Penalty effect (team + player) → YAC fails → Yards → Fumble → Time
        /// </summary>
        public static TestFluentSeedableRandom WithBlockingAndTacklePenalty(int airYards = 10)
        {
            return new TestFluentSeedableRandom()
                .PassProtectionCheck(0.7)
                .NextDouble(0.01)                      // BLOCKING PENALTY!
                .NextDouble(0.5)                       // Penalty effect: team selection
                .NextInt(5)                            // Penalty effect: player selection
                .QBPressureCheck(0.8)
                .ReceiverSelection(0.5)
                .PassTypeDetermination(0.6)
                .AirYards(airYards)
                .PassCompletionCheck(0.5)
                .NextDouble(0.01)                      // RECEIVER TACKLE PENALTY!
                .NextDouble(0.5)                       // Penalty effect: team selection
                .NextInt(5)                            // Penalty effect: player selection
                .YACOpportunityCheck(0.8)
                .ImmediateTackleYards(2)
                .NextDouble(0.99)                      // No fumble
                .ElapsedTimeRandomFactor(0.99);
        }

        #endregion

        #region Pressure Scenarios

        /// <summary>
        /// Completed pass under pressure - QB hurried but completes.
        /// Pressure reduces completion percentage.
        /// </summary>
        public static TestFluentSeedableRandom CompletedUnderPressure(int airYards = 10)
        {
            return CompletedPassImmediateTackle(airYards: airYards, pressure: true);
        }

        /// <summary>
        /// Incomplete pass due to pressure - QB rushed, pass fails.
        /// </summary>
        public static TestFluentSeedableRandom IncompleteUnderPressure()
        {
            return IncompletePass(withPressure: true);
        }

        #endregion

        #region Custom Builders

        /// <summary>
        /// Custom scenario builder for complete control over all random values.
        /// Use this for edge cases or when testing specific combinations not covered by other scenarios.
        ///
        /// IMPORTANT: When random sequences change (like adding pre-snap penalties), custom builders
        /// will need manual updates. Prefer using the named scenario methods above for maintainability.
        /// </summary>
        public static TestFluentSeedableRandom Custom(
            double protectionValue = 0.7,
            bool blockingPenalty = false,
            bool pressure = false,
            double passTypeValue = 0.6,
            int airYards = 10,
            bool completes = true,
            bool hasYAC = false,
            bool hasBigPlay = false,
            int bigPlayYards = 20,
            bool coveragePenalty = false,
            bool interception = false,
            bool fumble = false)
        {
            var rng = new TestFluentSeedableRandom();

            // Check if this is a sack scenario
            if (protectionValue >= 0.75)
            {
                // Sack path
                rng.PassProtectionCheck(protectionValue)
                    .NextDouble(blockingPenalty ? 0.01 : 0.99)
                    .SackYards(7)
                    .NextDouble(0.99)  // Roughing the passer
                    .NextDouble(fumble ? 0.01 : 0.99)
                    .ElapsedTimeRandomFactor(0.99);
            }
            else
            {
                // Pass attempt path
                rng.PassProtectionCheck(protectionValue)
                    .NextDouble(blockingPenalty ? 0.01 : 0.99)
                    .QBPressureCheck(pressure ? 0.2 : 0.8)
                    .ReceiverSelection(0.5)
                    .PassTypeDetermination(passTypeValue)
                    .AirYards(airYards)
                    .PassCompletionCheck(completes ? 0.5 : 0.9);

                if (completes)
                {
                    // Completion path
                    if (hasYAC)
                    {
                        rng.YACOpportunityCheck(0.3)
                            .YACRandomFactor(0.5)
                            .BigPlayCheck(hasBigPlay ? 0.02 : 0.9);

                        if (hasBigPlay)
                            rng.BigPlayBonusYards(bigPlayYards);

                        rng.NextDouble(0.99);  // Receiver tackle penalty
                    }
                    else
                    {
                        rng.NextDouble(0.99)   // Receiver tackle penalty
                            .YACOpportunityCheck(0.8)
                            .ImmediateTackleYards(2);
                    }

                    rng.NextDouble(fumble ? 0.01 : 0.99)
                        .ElapsedTimeRandomFactor(0.99);
                }
                else
                {
                    // Incomplete path
                    rng.NextDouble(coveragePenalty ? 0.01 : 0.99)
                        .InterceptionOccurredCheck(interception ? 0.01 : 0.99);

                    if (interception)
                    {
                        rng.InterceptionReturnBase(0.5)
                            .InterceptionReturnVariance(0.5)
                            .NextDouble(0.99)  // Tackle penalty on return
                            .NextDouble(fumble ? 0.01 : 0.99);
                    }

                    rng.ElapsedTimeRandomFactor(0.99);
                }
            }

            return rng;
        }

        #endregion
    }
}
