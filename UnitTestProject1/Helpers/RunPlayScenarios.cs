using System;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for run play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for run plays so that when new random checks are added
    /// (like pre-snap penalties), we only need to update these factory methods instead of hundreds of tests.
    ///
    /// USAGE: Instead of manually building random sequences in each test, call a factory method:
    ///   var rng = RunPlayScenarios.SimpleGain(yards: 5);
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    /// 1. QB check (NextDouble) - determines if QB scrambles or RB gets ball
    /// 2. Direction (NextInt 0-4) - run direction
    /// 3. Blocking check (NextDouble) - offensive line blocking success
    /// 4. Blocking penalty check (NextDouble) - offensive/defensive holding
    /// 5. Base yards random factor (NextDouble) - variance in yards calculation
    /// 6. Tackle break check (NextDouble) - ball carrier breaks tackle
    /// 7. [Conditional] Tackle break yards (NextInt 3-8) - only if tackle break succeeds
    /// 8. Breakaway check (NextDouble) - long run opportunity
    /// 9. [Conditional] Breakaway yards (NextInt 15-44) - only if breakaway succeeds
    /// 10. Tackle penalty check (NextDouble) - unnecessary roughness, etc.
    /// 11. Fumble check (NextDouble) - ball carrier fumbles
    /// 12. Elapsed time random factor (NextDouble) - play duration
    ///
    /// NOTE: When Phase 3 adds pre-snap penalties, we'll insert those checks at position 1,
    /// and all tests using these factories will automatically work.
    /// </summary>
    public static class RunPlayScenarios
    {
        #region Basic Scenarios

        /// <summary>
        /// Simple gain scenario - RB gets ball, moderate blocking, no special plays.
        /// Most common scenario for testing basic run play mechanics.
        ///
        /// Random sequence:
        /// 1. QB check (0.15) - RB gets ball (> 0.10 threshold)
        /// 2. Direction (2) - Middle
        /// 3. Blocking check (0.5) - Moderate success
        /// 4. Blocking penalty (0.99) - No penalty
        /// 5. Base yards factor - Calculated to achieve target yards
        /// 6. Tackle break (0.9) - No tackle break
        /// 7. Breakaway (0.9) - No breakaway
        /// 8. Tackle penalty (0.99) - No penalty
        /// 9. Fumble (0.99) - No fumble
        /// 10. Elapsed time (0.5) - ~6.5 seconds
        /// </summary>
        /// <param name="yards">Target yards to gain (will calculate appropriate random factor)</param>
        /// <param name="direction">Run direction (0-4), default 2 (Middle)</param>
        /// <returns>Configured TestFluentSeedableRandom for this scenario</returns>
        public static TestFluentSeedableRandom SimpleGain(int yards, int direction = 2)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(direction)                  // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.6)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);       // ~6.5 seconds
        }

        /// <summary>
        /// QB scramble scenario - QB keeps ball and runs.
        /// Used when QB check falls below 0.10 threshold.
        ///
        /// Random sequence: Same as SimpleGain but with QB check = 0.05
        /// </summary>
        /// <param name="yards">Target yards to gain</param>
        /// <param name="direction">Run direction, default 2 (Middle)</param>
        public static TestFluentSeedableRandom QBScramble(int yards, int direction = 2)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.05)                    // QB check (QB scrambles < 0.10)
                .NextInt(direction)                  // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.6)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);       // ~6.5 seconds
        }

        /// <summary>
        /// Tackle for loss scenario - defense stuffs the run for negative yards.
        /// Uses bad blocking and low base yards factor.
        ///
        /// Random sequence:
        /// - Bad blocking (0.7-0.8)
        /// - Low base yards factor (0.05-0.2) produces negative yards
        /// </summary>
        /// <param name="lossYards">Yards lost (positive number, will result in negative gain)</param>
        public static TestFluentSeedableRandom TackleForLoss(int lossYards = 2)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.8)               // Bad blocking (fails)
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.05)                    // Low base yards factor (negative result)
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Blocking Variance Scenarios

        /// <summary>
        /// Good blocking scenario - offensive line dominates, creates running lanes.
        /// Blocking check succeeds (< 0.5 threshold).
        /// </summary>
        /// <param name="yards">Target yards to gain</param>
        public static TestFluentSeedableRandom GoodBlocking(int yards)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.4)               // Good blocking (succeeds)
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.7)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Bad blocking scenario - offensive line fails, RB hit in backfield.
        /// Blocking check fails (>= 0.5 threshold).
        /// </summary>
        /// <param name="yards">Target yards to gain (will be reduced by bad blocking)</param>
        public static TestFluentSeedableRandom BadBlocking(int yards)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.6)               // Bad blocking (fails)
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.7)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Special Play Scenarios

        /// <summary>
        /// Tackle break scenario - ball carrier breaks initial tackle for extra yards.
        ///
        /// Random sequence includes tackle break yards (NextInt 3-8).
        /// NOTE: This adds an extra random value compared to simple scenarios.
        /// </summary>
        /// <param name="baseYards">Documentation only - yards before tackle break (use blockingValue and baseYardsFactor to control)</param>
        /// <param name="tackleBreakYards">Additional yards from breaking tackle (3-8)</param>
        /// <param name="blockingValue">Blocking check value (default 0.4 = good blocking)</param>
        /// <param name="baseYardsFactor">Base yards random factor (default 0.68)</param>
        public static TestFluentSeedableRandom TackleBreak(int baseYards, int tackleBreakYards = 5,
            double blockingValue = 0.4, double baseYardsFactor = 0.68)
        {
            if (tackleBreakYards < 3 || tackleBreakYards > 8)
                throw new ArgumentOutOfRangeException(nameof(tackleBreakYards),
                    "Tackle break yards must be 3-8 per TackleBreakYardsSkillsCheckResult");

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(blockingValue)     // Blocking (customizable)
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(baseYardsFactor)         // Base yards random factor (customizable)
                .TackleBreakCheck(0.1)               // Tackle break succeeds!
                .NextInt(tackleBreakYards)           // EXTRA: Tackle break yards
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Breakaway run scenario - ball carrier breaks free for significant yardage.
        ///
        /// Random sequence includes breakaway yards (NextInt 15-44).
        /// NOTE: This adds an extra random value compared to simple scenarios.
        /// </summary>
        /// <param name="baseYards">Yards before breakaway</param>
        /// <param name="breakawayYards">Additional yards from breakaway (15-44)</param>
        public static TestFluentSeedableRandom Breakaway(int baseYards, int breakawayYards = 30)
        {
            if (breakawayYards < 15 || breakawayYards > 44)
                throw new ArgumentOutOfRangeException(nameof(breakawayYards),
                    "Breakaway yards must be 15-44 per BreakawayYardsSkillsCheckResult");

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.5)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.05)                // Breakaway succeeds!
                .NextInt(breakawayYards)             // EXTRA: Breakaway yards
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Maximum yardage scenario - everything goes right: good blocking, tackle break, AND breakaway.
        ///
        /// Random sequence includes BOTH tackle break yards AND breakaway yards.
        /// NOTE: This adds TWO extra random values.
        /// </summary>
        /// <param name="tackleBreakYards">Tackle break bonus (3-8)</param>
        /// <param name="breakawayYards">Breakaway bonus (15-44)</param>
        public static TestFluentSeedableRandom MaximumYardage(int tackleBreakYards = 8, int breakawayYards = 40)
        {
            if (tackleBreakYards < 3 || tackleBreakYards > 8)
                throw new ArgumentOutOfRangeException(nameof(tackleBreakYards));
            if (breakawayYards < 15 || breakawayYards > 44)
                throw new ArgumentOutOfRangeException(nameof(breakawayYards));

            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.3)               // Great blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.9)                     // High base yards factor
                .TackleBreakCheck(0.05)              // Tackle break succeeds!
                .NextInt(tackleBreakYards)           // EXTRA: Tackle break yards
                .BreakawayCheck(0.02)                // Breakaway succeeds!
                .NextInt(breakawayYards)             // EXTRA: Breakaway yards
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Fumble Scenarios

        /// <summary>
        /// Fumble scenario - ball carrier fumbles after gaining yards.
        ///
        /// Random sequence: Same as SimpleGain but fumble check = 0.01 (occurs).
        /// </summary>
        /// <param name="yardsBeforeFumble">Yards gained before fumble</param>
        public static TestFluentSeedableRandom Fumble(int yardsBeforeFumble)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.6)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.01)                    // FUMBLE! (< fumble threshold)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Blocking penalty scenario - offensive holding during run.
        ///
        /// Random sequence: Same as SimpleGain but blocking penalty = 0.01 (occurs).
        /// This triggers BlockingPenaltyOccurredSkillsCheck to detect holding.
        /// </summary>
        /// <param name="yards">Yards that would have been gained</param>
        public static TestFluentSeedableRandom WithBlockingPenalty(int yards)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.01)                    // BLOCKING PENALTY! (< penalty threshold)
                .NextDouble(0.6)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.99)                    // No tackle penalty
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Tackle penalty scenario - unnecessary roughness on tackle.
        ///
        /// Random sequence: Same as SimpleGain but tackle penalty = 0.01 (occurs).
        /// This triggers TacklePenaltyOccurredSkillsCheck with BallCarrier context.
        /// </summary>
        /// <param name="yards">Yards gained before penalty</param>
        public static TestFluentSeedableRandom WithTacklePenalty(int yards)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.15)                    // QB check (RB gets ball)
                .NextInt(2)                          // Direction
                .RunBlockingCheck(0.5)               // Moderate blocking
                .NextDouble(0.99)                    // No blocking penalty
                .NextDouble(0.6)                     // Base yards random factor
                .TackleBreakCheck(0.9)               // No tackle break
                .BreakawayCheck(0.9)                 // No breakaway
                .NextDouble(0.01)                    // TACKLE PENALTY! (< penalty threshold)
                .NextDouble(0.99)                    // No fumble
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Custom Builders

        /// <summary>
        /// Custom scenario builder for complete control over all random values.
        /// Use this for edge cases or when testing specific combinations not covered by other scenarios.
        ///
        /// IMPORTANT: This method is useful when you need precise control, but prefer using
        /// the named scenario methods above for maintainability. When random sequences change
        /// (like adding pre-snap penalties), custom builders will need manual updates.
        /// </summary>
        public static TestFluentSeedableRandom Custom(
            bool qbScrambles = false,
            int direction = 2,
            double blockingCheckValue = 0.5,
            bool blockingPenalty = false,
            double baseYardsFactor = 0.6,
            bool tackleBreak = false,
            int tackleBreakYards = 5,
            bool breakaway = false,
            int breakawayYards = 30,
            bool tacklePenalty = false,
            bool fumble = false,
            double elapsedTimeFactor = 0.5)
        {
            var rng = new TestFluentSeedableRandom()
                .NextDouble(qbScrambles ? 0.05 : 0.15)
                .NextInt(direction)
                .RunBlockingCheck(blockingCheckValue)
                .NextDouble(blockingPenalty ? 0.01 : 0.99)
                .NextDouble(baseYardsFactor)
                .TackleBreakCheck(tackleBreak ? 0.1 : 0.9);

            if (tackleBreak)
                rng.NextInt(tackleBreakYards);

            rng.BreakawayCheck(breakaway ? 0.05 : 0.9);

            if (breakaway)
                rng.NextInt(breakawayYards);

            rng.NextDouble(tacklePenalty ? 0.01 : 0.99)
                .NextDouble(fumble ? 0.01 : 0.99)
                .ElapsedTimeRandomFactor(elapsedTimeFactor);

            return rng;
        }

        #endregion
    }
}
