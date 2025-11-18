using System;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for field goal play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for field goal and extra point plays. Field goals have multiple execution paths
    /// (bad snap, blocked kick, made, missed) with different random value sequences.
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    ///
    /// BAD SNAP PATH:
    /// 1. Bad snap check (< ~0.01-0.05 based on snapper blocking skill)
    /// 2. Bad snap yards - base loss (NextDouble for -5 to -15 yards)
    /// 3. Bad snap yards - random factor (NextDouble for ±2.5 yards)
    /// 4. Elapsed time (NextDouble, 4-7 seconds)
    /// TOTAL: 4 random values
    ///
    /// BLOCKED FIELD GOAL - OFFENSE RECOVERS PATH:
    /// 1. Bad snap check (succeeds, >= threshold)
    /// 2. Block check (< ~0.015-0.065 based on distance, longer = higher)
    /// 3. Recovery check (>= 0.5 = offense recovers)
    /// 4. Recovery yards (NextDouble for -5 to -15 yards)
    /// 5. Elapsed time (NextDouble, 3-6 seconds)
    /// TOTAL: 5 random values
    ///
    /// BLOCKED FIELD GOAL - DEFENSE RECOVERS PATH:
    /// 1. Bad snap check (succeeds)
    /// 2. Block check (fails, kick is blocked)
    /// 3. Recovery check (< 0.5 = defense recovers)
    /// 4. Return yards (NextDouble via BlockedFieldGoalReturnYardsSkillsCheckResult, -5 to 100 yards)
    /// 5. Elapsed time (NextDouble, 3-6 seconds)
    /// TOTAL: 5 random values
    ///
    /// NORMAL FIELD GOAL - MADE PATH:
    /// 1. Bad snap check (succeeds)
    /// 2. Block check (succeeds)
    /// 3. Make check (NextDouble, < probability based on distance and kicker skill)
    /// 4. Kicker penalty check (roughing/running into kicker)
    /// 5. [Conditional] Penalty effect (NextDouble team + NextInt player) if penalty occurs
    /// 6. Elapsed time (NextDouble, 2-3 seconds)
    /// TOTAL: 5 random values (7 if penalty)
    /// NOTE: No miss direction consumed since kick is good
    ///
    /// NORMAL FIELD GOAL - MISSED PATH:
    /// 1. Bad snap check (succeeds)
    /// 2. Block check (succeeds)
    /// 3. Make check (fails, >= probability)
    /// 4. Kicker penalty check (roughing/running into kicker)
    /// 5. [Conditional] Penalty effect if penalty occurs
    /// 6. Miss direction (NextDouble for wide right/left/short) - ONLY consumed if missed
    /// 7. Elapsed time (NextDouble, 2-3 seconds)
    /// TOTAL: 6 random values (8 if penalty)
    ///
    /// IMPORTANT NOTES:
    /// - Extra points (PAT) are 20-yard attempts with ~98% make rate
    /// - Block probability increases with distance: 1.5% (short) to 6.5% (55+ yards)
    /// - Miss direction is ONLY consumed when kick misses
    /// - Kicker penalties can occur on any normal attempt (made or missed)
    /// </summary>
    public static class FieldGoalPlayScenarios
    {
        #region Extra Point (PAT) Scenarios

        /// <summary>
        /// Extra point made - successful PAT after touchdown.
        /// Most common scenario, ~98% success rate.
        ///
        /// Random sequence: Bad snap → Block → Make → Kicker penalty → Elapsed time
        /// TOTAL: 5 random values
        /// </summary>
        public static TestFluentSeedableRandom ExtraPointMade()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block (> 1.5% for PAT)
                .NextDouble(0.5)                   // Make (98% for PAT)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Extra point missed - rare PAT miss.
        ///
        /// Random sequence: Bad snap → Block → Make fails → Kicker penalty → Miss direction → Elapsed time
        /// TOTAL: 6 random values
        /// </summary>
        public static TestFluentSeedableRandom ExtraPointMissed()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.99)                  // Miss (rare for PAT)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5)                   // Miss direction
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Short Field Goal Scenarios (< 30 yards)

        /// <summary>
        /// Short field goal made - 18-30 yard attempt, high success rate.
        ///
        /// Random sequence: Bad snap → Block → Make → Kicker penalty → Elapsed time
        /// TOTAL: 5 random values
        /// </summary>
        public static TestFluentSeedableRandom ShortFieldGoalMade()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.5)                   // Make (high probability)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Medium Field Goal Scenarios (30-50 yards)

        /// <summary>
        /// Medium field goal made - 30-50 yard attempt.
        /// Typical game-winning field goal scenario.
        ///
        /// Random sequence: Bad snap → Block → Make → Kicker penalty → Elapsed time
        /// </summary>
        /// <param name="makeValue">Make check value (0-1), determines success based on kicker skill and distance</param>
        public static TestFluentSeedableRandom MediumFieldGoalMade(double makeValue = 0.4)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(makeValue)             // Make check
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Medium field goal missed - 30-50 yard attempt fails.
        ///
        /// Random sequence: Bad snap → Block → Make fails → Kicker penalty → Miss direction → Elapsed time
        /// TOTAL: 6 random values
        /// </summary>
        /// <param name="missDirection">Miss direction value (0-1): <0.4=wide right, 0.4-0.8=wide left, >0.8=short</param>
        public static TestFluentSeedableRandom MediumFieldGoalMissed(double missDirection = 0.3)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.95)                  // Miss
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(missDirection)         // Miss direction
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Long Field Goal Scenarios (50-60 yards)

        /// <summary>
        /// Long field goal made - 50-60 yard attempt by excellent kicker.
        /// Requires kicker with high kicking skill (75+).
        ///
        /// Random sequence: Bad snap → Block → Make → Kicker penalty → Elapsed time
        /// </summary>
        /// <param name="makeValue">Make check value (0-1), typically 0.2-0.4 for long kicks</param>
        public static TestFluentSeedableRandom LongFieldGoalMade(double makeValue = 0.2)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(makeValue)             // Make check (excellent kicker needed)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Long field goal missed - 50-60 yard attempt fails.
        /// More likely to miss than make at this distance.
        ///
        /// Random sequence: Bad snap → Block → Make fails → Kicker penalty → Miss direction → Elapsed time
        /// </summary>
        /// <param name="missDirection">Miss direction value (0-1)</param>
        public static TestFluentSeedableRandom LongFieldGoalMissed(double missDirection = 0.85)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.8)                   // Miss (likely at long distance)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(missDirection)         // Miss direction (often short)
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Bad Snap Scenarios

        /// <summary>
        /// Bad snap - holder recovers but loses yards.
        ///
        /// Random sequence: Bad snap occurs → Bad snap yards (base + random) → Elapsed time
        /// TOTAL: 4 random values
        /// </summary>
        /// <param name="baseLoss">Base loss value (0-1), 0.5 = ~-10 yards</param>
        /// <param name="randomFactor">Random factor (0-1), 0.5 = 0 yard adjustment</param>
        public static TestFluentSeedableRandom BadSnap(double baseLoss = 0.5, double randomFactor = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.01)                  // BAD SNAP! (< ~2.2% with 70 blocking)
                .NextDouble(baseLoss)              // Base loss (-5 to -15)
                .NextDouble(randomFactor)          // Random factor (±2.5)
                .NextDouble(0.5);                  // Elapsed time (4-7 seconds)
        }

        /// <summary>
        /// Bad snap resulting in safety - snap rolls into own end zone.
        /// </summary>
        public static TestFluentSeedableRandom BadSnapSafety()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.01)                  // BAD SNAP!
                .NextDouble(0.9)                   // Large loss (near max)
                .NextDouble(0.9)                   // Random factor pushes into end zone
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Blocked Kick Scenarios

        /// <summary>
        /// Blocked field goal recovered by offense - kicking team falls on it for loss.
        ///
        /// Random sequence: No bad snap → BLOCK → Offense recovers → Recovery yards → Elapsed time
        /// TOTAL: 5 random values
        /// </summary>
        /// <param name="recoveryYards">Recovery yards value (0-1), 0.5 = ~-10 yard loss</param>
        public static TestFluentSeedableRandom BlockedFieldGoalOffenseRecovers(double recoveryYards = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.6)                   // Offense recovers (>= 50%)
                .NextDouble(recoveryYards)         // Recovery yards (-5 to -15)
                .NextDouble(0.5);                  // Elapsed time (3-6 seconds)
        }

        /// <summary>
        /// Blocked field goal recovered by defense - defensive player scoops it up.
        ///
        /// Random sequence: No bad snap → BLOCK → Defense recovers → Return yards → Elapsed time
        /// TOTAL: 5 random values
        /// </summary>
        /// <param name="returnYards">Return yardage value (0-1), 0.5 = ~15 yards, 0.99 = possible TD</param>
        public static TestFluentSeedableRandom BlockedFieldGoalDefenseRecovers(double returnYards = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.3)                   // Defense recovers (< 50%)
                .NextDouble(returnYards)           // Return yards (-5 to 100)
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Blocked field goal returned for touchdown - scoop and score.
        /// </summary>
        public static TestFluentSeedableRandom BlockedFieldGoalTouchdown()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.3)                   // Defense recovers
                .NextDouble(0.99)                  // Excellent return for TD
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Blocked field goal recovered by offense in end zone - safety.
        /// </summary>
        public static TestFluentSeedableRandom BlockedFieldGoalSafety()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.6)                   // Offense recovers
                .NextDouble(0.8)                   // Large loss into end zone
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Blocked field goal by defense resulting in safety - defense runs into kicking team end zone.
        /// Extremely rare scenario.
        /// </summary>
        public static TestFluentSeedableRandom BlockedFieldGoalDefenseSafety()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.3)                   // Defense recovers
                .NextDouble(0.01)                  // Very low return (negative/into end zone)
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Long kick with higher block probability - 55+ yard attempts are easier to block.
        /// Block probability: 6.5% vs 1.5% for short kicks.
        /// </summary>
        public static TestFluentSeedableRandom LongKickBlocked()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.03)                  // Would block long kick (< 6.5%)
                .NextDouble(0.3)                   // Defense recovers
                .NextDouble(0.5)                   // Return yards
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Field goal with roughing the kicker penalty - defender hits kicker.
        /// Can occur on made or missed attempts.
        ///
        /// Random sequence: Normal path but kicker penalty = 0.01 + penalty effect randoms.
        /// NOTE: This adds 2 extra randoms (team selection + player selection).
        /// </summary>
        /// <param name="kickIsMade">Whether the kick itself is good (before penalty consideration)</param>
        public static TestFluentSeedableRandom WithRoughingTheKicker(bool kickIsMade = true)
        {
            var rng = new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(kickIsMade ? 0.5 : 0.95) // Make/miss check
                .NextDouble(0.01)                  // ROUGHING THE KICKER!
                .NextDouble(0.5)                   // Penalty effect: team selection
                .NextInt(5);                       // Penalty effect: player selection

            if (!kickIsMade)
            {
                rng.NextDouble(0.5);               // Miss direction (only if missed)
            }

            rng.NextDouble(0.5);                   // Elapsed time

            return rng;
        }

        #endregion

        #region Edge Case Scenarios

        /// <summary>
        /// Extremely long field goal (65+ yards) - very low make probability.
        /// Requires backup punter or emergency kicker scenario.
        /// </summary>
        public static TestFluentSeedableRandom ExtremelyLongFieldGoal()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.99)                  // Likely miss from 116 yards
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.5)                   // Miss direction
                .NextDouble(0.5);                  // Elapsed time
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
            bool badSnap = false,
            bool blocked = false,
            bool offenseRecoversBlock = false,
            bool kickIsMade = true,
            bool kickerPenalty = false,
            double returnYards = 0.5,
            double missDirection = 0.5)
        {
            var rng = new TestFluentSeedableRandom();

            // Bad snap path
            if (badSnap)
            {
                rng.NextDouble(0.01)               // Bad snap occurs
                    .NextDouble(0.5)               // Base loss
                    .NextDouble(0.5)               // Random factor
                    .NextDouble(0.5);              // Elapsed time
                return rng;
            }

            // No bad snap
            rng.NextDouble(0.99);                  // No bad snap

            // Blocked kick path
            if (blocked)
            {
                rng.NextDouble(0.01)               // BLOCKED!
                    .NextDouble(offenseRecoversBlock ? 0.6 : 0.3); // Recovery

                if (offenseRecoversBlock)
                {
                    rng.NextDouble(0.5);           // Recovery yards
                }
                else
                {
                    rng.NextDouble(returnYards);   // Return yards
                }

                rng.NextDouble(0.5);               // Elapsed time
                return rng;
            }

            // Normal kick path
            rng.NextDouble(0.99)                   // No block
                .NextDouble(kickIsMade ? 0.5 : 0.95) // Make/miss check
                .NextDouble(kickerPenalty ? 0.01 : 0.99); // Kicker penalty

            if (kickerPenalty)
            {
                rng.NextDouble(0.5).NextInt(5);    // Penalty effect
            }

            if (!kickIsMade)
            {
                rng.NextDouble(missDirection);     // Miss direction (only if missed)
            }

            rng.NextDouble(0.5);                   // Elapsed time

            return rng;
        }

        #endregion
    }
}
