using System;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for punt play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for punt plays. Punts have multiple execution paths
    /// (bad snap, blocked punt, touchback, downed, fair catch, muffed catch, return) with different random sequences.
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    ///
    /// BAD SNAP PATH:
    /// 1. Bad snap check (< ~0.01-0.05 based on snapper blocking skill)
    /// 2. Bad snap yards - base loss (NextDouble for -5 to -20 yards)
    /// 3. Bad snap yards - random factor (NextDouble for ±2.5 yards)
    /// 4. Elapsed time (NextDouble, 4-8 seconds)
    /// TOTAL: 4 random values
    ///
    /// BLOCKED PUNT - OFFENSE RECOVERS PATH:
    /// 1. Bad snap check (succeeds, >= threshold)
    /// 2. Block check (< ~0.01-0.30 based on skills and snap quality)
    /// 3. Recovery check (< 0.5 = offense recovers)
    /// 4. Recovery yards (NextDouble for -5 to -10 calculation)
    /// 5. Elapsed time (NextDouble, 3-6 seconds)
    /// TOTAL: 5 random values
    ///
    /// BLOCKED PUNT - DEFENSE RECOVERS PATH:
    /// 1. Bad snap check (succeeds)
    /// 2. Block check (fails, punt is blocked)
    /// 3. Recovery check (>= 0.5 = defense recovers)
    /// 4. Ball bounce base (NextDouble for -10 to +15 yards)
    /// 5. Ball bounce random factor (NextDouble for ±5 yards)
    /// 6. Elapsed time (NextDouble, 3-6 seconds)
    /// TOTAL: 6 random values
    ///
    /// TOUCHBACK PATH:
    /// 1. Bad snap check (succeeds)
    /// 2. Block check (succeeds)
    /// 3. Punt distance (NextDouble for distance calculation)
    /// 4. Hang time (NextDouble for hang time calculation)
    /// 5. Kicker penalty check (roughing/running into kicker)
    /// 6. [Conditional] Penalty effect (NextDouble team + NextInt player) if penalty occurs
    /// NOTE: Elapsed time is constant (hang time + 0.5), not random
    /// TOTAL: 5 random values (7 if penalty)
    ///
    /// OUT OF BOUNDS PATH:
    /// 1-5. Same as touchback
    /// 6. Out of bounds check (< threshold based on landing spot)
    /// NOTE: Elapsed time is constant (hang time + 0.5)
    /// TOTAL: 6 random values (8 if penalty)
    ///
    /// DOWNED PATH:
    /// 1-5. Same as touchback
    /// 6. Out of bounds check (fails)
    /// 7. Downed check (< threshold based on landing spot and hang time)
    /// NOTE: Elapsed time is constant (hang time + 1.0)
    /// TOTAL: 7 random values (9 if penalty)
    ///
    /// FAIR CATCH PATH:
    /// 1-7. Same as downed
    /// 8. Fair catch check (< 0.25-0.55 based on hang time and field position)
    /// 9. Elapsed time (NextDouble for hang time + 0.5)
    /// TOTAL: 9 random values (11 if penalty)
    ///
    /// MUFFED CATCH - RECEIVING TEAM RECOVERS PATH:
    /// 1-7. Same as downed
    /// 8. Fair catch check (fails)
    /// 9. Muff check (< muff threshold based on returner catching and hang time)
    /// 10. Recovery check (< 0.6 = receiving team recovers their muff)
    /// 11. Recovery yards (NextDouble for -5 to +5 yards from muff spot)
    /// 12. Elapsed time (NextDouble for hang time + 2-4 seconds)
    /// TOTAL: 12 random values (14 if penalty)
    ///
    /// MUFFED CATCH - PUNTING TEAM RECOVERS PATH:
    /// 1-9. Same as muffed (receiving team)
    /// 10. Recovery check (>= 0.6 = punting team recovers)
    /// 11. Elapsed time (NextDouble for hang time + 2-4 seconds)
    /// NOTE: No possession change when punting team recovers
    /// TOTAL: 11 random values (13 if penalty)
    ///
    /// NORMAL RETURN PATH:
    /// 1-7. Same as downed
    /// 8. Fair catch check (fails)
    /// 9. Muff check (fails)
    /// 10. Return yards (NextDouble for return calculation)
    /// 11. [Conditional] Returner tackle penalty check - only if returnYards > 0
    /// 12. [Conditional] Penalty effect (NextDouble team + NextInt player) if penalty occurs
    /// 13. Elapsed time (NextDouble for hang time + 2-6 seconds)
    /// TOTAL: 12 random values (14 if tackle penalty), 11 if returnYards <= 0
    ///
    /// RETURN TOUCHDOWN PATH:
    /// Same as normal return - tackle penalty check still occurs before TD is detected
    /// TOTAL: 12 random values (14 if tackle penalty)
    ///
    /// IMPORTANT NOTES:
    /// - Kicker penalties can occur on any path that gets past the block check
    /// - Return penalties only checked if returnYards > 0
    /// - Some paths have constant elapsed time, others have random elapsed time
    /// - Bad snap and blocked punt return early, skipping later checks
    /// </summary>
    public static class PuntPlayScenarios
    {
        #region Normal Punt Scenarios

        /// <summary>
        /// Normal punt with return - standard punt with moderate return.
        /// Most common punt scenario.
        ///
        /// Random sequence: Bad snap → Block → Distance → Hang time → Kicker penalty →
        /// Out of bounds → Downed → Fair catch → Muff → Return yards → Returner penalty → Elapsed time
        /// </summary>
        /// <param name="puntDistance">Punt distance value (0-1), 0.5 = ~45 yards</param>
        /// <param name="returnYards">Return yardage value (0-1), 0.5 = ~5-10 yards</param>
        public static TestFluentSeedableRandom NormalReturn(double puntDistance = 0.6, double returnYards = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time random
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.95)                  // No muff
                .NextDouble(returnYards)           // Return yards
                .NextDouble(0.99)                  // No returner penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Short punt - weak kick or intentional short punt.
        /// </summary>
        public static TestFluentSeedableRandom ShortPunt(double returnYards = 0.5)
        {
            return NormalReturn(puntDistance: 0.2, returnYards: returnYards);
        }

        /// <summary>
        /// Long punt - good punter, strong kick.
        /// </summary>
        public static TestFluentSeedableRandom LongPunt(double returnYards = 0.3)
        {
            return NormalReturn(puntDistance: 0.85, returnYards: returnYards);
        }

        /// <summary>
        /// Return for touchdown - great return all the way to end zone.
        ///
        /// Random sequence: Same as normal return, including tackle penalty check
        /// (penalty check happens before TD is detected).
        /// </summary>
        public static TestFluentSeedableRandom ReturnTouchdown()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.05)                  // Short punt (easier to return for TD)
                .NextDouble(0.3)                   // Short hang time (poor coverage)
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.95)                  // No muff
                .NextDouble(0.95)                  // Excellent return for TD
                .NextDouble(0.99)                  // No returner penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Bad Snap Scenarios

        /// <summary>
        /// Bad snap - punter recovers but loses yards.
        ///
        /// Random sequence: Bad snap occurs → Bad snap yards (base + random) → Elapsed time
        /// NOTE: Much shorter sequence than normal punt - only 4 random values.
        /// </summary>
        /// <param name="baseLoss">Base loss value (0-1), 0.3 = ~-9.5 yards</param>
        /// <param name="randomFactor">Random factor (0-1), 0.5 = 0 yard adjustment</param>
        public static TestFluentSeedableRandom BadSnap(double baseLoss = 0.3, double randomFactor = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.01)                  // BAD SNAP! (< ~2.2% with 70 blocking)
                .NextDouble(baseLoss)              // Base loss (-5 to -20)
                .NextDouble(randomFactor)          // Random factor (±2.5)
                .NextDouble(0.7);                  // Elapsed time (4-8 seconds)
        }

        /// <summary>
        /// Bad snap resulting in safety - snap into own end zone.
        /// </summary>
        public static TestFluentSeedableRandom BadSnapSafety()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.01)                  // BAD SNAP!
                .NextDouble(0.9)                   // Large loss (near max)
                .NextDouble(0.8)                   // Random factor pushes into end zone
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Blocked Punt Scenarios

        /// <summary>
        /// Blocked punt recovered by offense - punting team falls on it for loss.
        ///
        /// Random sequence: No bad snap → BLOCK → Offense recovers → Recovery yards → Elapsed time
        /// TOTAL: 5 random values
        /// </summary>
        /// <param name="recoveryYards">Recovery yards value (0-1), 0.7 = ~-8.5 yard loss</param>
        public static TestFluentSeedableRandom BlockedPuntOffenseRecovers(double recoveryYards = 0.7)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.3)                   // Offense recovers (< 50%)
                .NextDouble(recoveryYards)         // Recovery yards (-5 to -10)
                .NextDouble(0.5);                  // Elapsed time (3-6 seconds)
        }

        /// <summary>
        /// Blocked punt recovered by defense - defensive player scoops it up.
        ///
        /// Random sequence: No bad snap → BLOCK → Defense recovers → Ball bounce base →
        /// Ball bounce random → Elapsed time
        /// TOTAL: 6 random values
        /// </summary>
        /// <param name="baseBounce">Base bounce value (0-1), 0.5 = ~2.5 yards forward</param>
        /// <param name="randomFactor">Random factor (0-1), 0.6 = +1 yard</param>
        public static TestFluentSeedableRandom BlockedPuntDefenseRecovers(
            double baseBounce = 0.5,
            double randomFactor = 0.6)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.6)                   // Defense recovers (>= 50%)
                .NextDouble(baseBounce)            // Ball bounce base (-10 to +15)
                .NextDouble(randomFactor)          // Random factor (±5)
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Blocked punt recovered for touchdown - defense recovers in end zone.
        /// </summary>
        public static TestFluentSeedableRandom BlockedPuntTouchdown()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.6)                   // Defense recovers
                .NextDouble(0.7)                   // Ball bounces forward
                .NextDouble(0.5)                   // Random factor
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Blocked punt recovered by offense in end zone - safety.
        /// </summary>
        public static TestFluentSeedableRandom BlockedPuntSafety()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.01)                  // BLOCKED!
                .NextDouble(0.3)                   // Offense recovers
                .NextDouble(0.05)                  // Large loss into end zone
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Touchback Scenarios

        /// <summary>
        /// Touchback - punt into end zone, ball placed at 20-yard line.
        ///
        /// Random sequence: No bad snap → No block → Punt distance (very long) →
        /// Hang time → Kicker penalty
        /// NOTE: Elapsed time is constant (hang time + 0.5), not random.
        /// TOTAL: 5 random values (7 if penalty)
        /// </summary>
        /// <param name="puntDistance">Punt distance (0-1), 0.95+ for touchback</param>
        public static TestFluentSeedableRandom Touchback(double puntDistance = 0.95)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(puntDistance)          // Very deep punt (into end zone)
                .NextDouble(0.9)                   // Good hang time
                .NextDouble(0.99);                 // No kicker penalty
        }

        #endregion

        #region Out of Bounds Scenarios

        /// <summary>
        /// Out of bounds punt - kick goes OOB, no return.
        ///
        /// Random sequence: No bad snap → No block → Distance → Hang time →
        /// Kicker penalty → OUT OF BOUNDS check
        /// TOTAL: 6 random values (8 if penalty)
        /// </summary>
        public static TestFluentSeedableRandom OutOfBounds()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.1);                  // OUT OF BOUNDS!
        }

        #endregion

        #region Downed Scenarios

        /// <summary>
        /// Punt downed by punting team - coverage team downs it, no return.
        ///
        /// Random sequence: No bad snap → No block → Distance → Hang time →
        /// Kicker penalty → Not OOB → DOWNED
        /// TOTAL: 7 random values (9 if penalty)
        /// </summary>
        /// <param name="puntDistance">Punt distance (0-1), 0.85 = long punt likely to be downed</param>
        public static TestFluentSeedableRandom Downed(double puntDistance = 0.85)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.7)                   // Good hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.2);                  // DOWNED!
        }

        /// <summary>
        /// Coffin corner punt - downed near goal line (inside 5-yard line).
        /// </summary>
        public static TestFluentSeedableRandom CoffinCorner()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.31)                  // Precise distance to land near goal
                .NextDouble(0.7)                   // Good hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.05);                 // DOWNED near goal line
        }

        #endregion

        #region Fair Catch Scenarios

        /// <summary>
        /// Fair catch - returner signals and catches without return.
        ///
        /// Random sequence: No bad snap → No block → Distance → Hang time →
        /// Kicker penalty → Not OOB → Not downed → FAIR CATCH → Elapsed time
        /// TOTAL: 9 random values (11 if penalty)
        /// </summary>
        /// <param name="puntDistance">Punt distance (0-1)</param>
        public static TestFluentSeedableRandom FairCatch(double puntDistance = 0.65)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.9)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.2)                   // FAIR CATCH! (< 0.55 threshold)
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Fair catch deep in own territory - high probability situation.
        /// </summary>
        public static TestFluentSeedableRandom FairCatchDeep()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.63)                  // 52-yard punt lands deep
                .NextDouble(0.9)                   // Good hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.6)                   // Not downed
                .NextDouble(0.15)                  // FAIR CATCH! (60% chance deep)
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Muffed Catch Scenarios

        /// <summary>
        /// Muffed catch recovered by receiving team - returner muffs but recovers.
        ///
        /// Random sequence: Full path to muff → Muff occurs → Receiving team recovers →
        /// Recovery yards → Elapsed time
        /// TOTAL: 12 random values (14 if penalty)
        /// </summary>
        /// <param name="recoveryYards">Recovery yards (0-1), 0.3 = ~-2 yards from muff</param>
        public static TestFluentSeedableRandom MuffReceivingTeamRecovers(double recoveryYards = 0.3)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.02)                  // MUFFED! (< muff threshold)
                .NextDouble(0.4)                   // Receiving team recovers (< 60%)
                .NextDouble(recoveryYards)         // Recovery yards (-5 to +5)
                .NextDouble(0.5);                  // Elapsed time (hang + 2-4 seconds)
        }

        /// <summary>
        /// Muffed catch recovered by punting team - great special teams play.
        ///
        /// Random sequence: Same as receiving team muff but no recovery yards random
        /// TOTAL: 11 random values (13 if penalty)
        /// </summary>
        public static TestFluentSeedableRandom MuffPuntingTeamRecovers()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.03)                  // MUFFED!
                .NextDouble(0.7)                   // Punting team recovers (>= 60%)
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Punt with roughing the kicker penalty - defender hits punter.
        ///
        /// Random sequence: Normal punt path but kicker penalty = 0.01 + penalty effect randoms.
        /// NOTE: This adds 2 extra randoms (team selection + player selection).
        /// </summary>
        public static TestFluentSeedableRandom WithRoughingTheKicker()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.95)                  // Touchback distance
                .NextDouble(0.9)                   // Hang time
                .NextDouble(0.01)                  // ROUGHING THE KICKER!
                .NextDouble(0.5)                   // Penalty effect: team selection
                .NextInt(5);                       // Penalty effect: player selection
        }

        /// <summary>
        /// Punt return with tackle penalty - unnecessary roughness on returner.
        ///
        /// Random sequence: Normal return but returner penalty = 0.01 + penalty effect randoms.
        /// </summary>
        public static TestFluentSeedableRandom WithReturnerTacklePenalty()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.95)                  // No muff
                .NextDouble(0.5)                   // Return yards (> 0 for penalty check)
                .NextDouble(0.01)                  // RETURNER TACKLE PENALTY!
                .NextDouble(0.5)                   // Penalty effect: team selection
                .NextInt(5)                        // Penalty effect: player selection
                .NextDouble(0.5);                  // Elapsed time
        }

        #endregion

        #region Edge Case Scenarios

        /// <summary>
        /// Shanked punt - extremely short punt due to poor execution.
        /// Requires weak punter (low kicking skill).
        /// </summary>
        public static TestFluentSeedableRandom ShankedPunt()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.0)                   // Minimum punt distance (shanked)
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.95)                  // No muff
                .NextDouble(0.5)                   // Return yards
                .NextDouble(0.99)                  // No returner penalty
                .NextDouble(0.5);                  // Elapsed time
        }

        /// <summary>
        /// Return with negative yards - returner tackled behind catch point.
        /// </summary>
        public static TestFluentSeedableRandom NegativeReturn()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.99)                  // No bad snap
                .NextDouble(0.99)                  // No block
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(0.99)                  // No kicker penalty
                .NextDouble(0.8)                   // Not out of bounds
                .NextDouble(0.9)                   // Not downed
                .NextDouble(0.9)                   // Not fair catch
                .NextDouble(0.95)                  // No muff
                .NextDouble(0.0)                   // Minimal/negative return
                .NextDouble(0.5);                  // Elapsed time (no penalty check if return <= 0)
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
            double puntDistance = 0.6,
            bool kickerPenalty = false,
            bool touchback = false,
            bool outOfBounds = false,
            bool downed = false,
            bool fairCatch = false,
            bool muff = false,
            bool receivingTeamRecoversMuff = true,
            double returnYards = 0.5,
            bool returnerPenalty = false)
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

            // Blocked punt path
            if (blocked)
            {
                rng.NextDouble(0.01)               // BLOCKED!
                    .NextDouble(offenseRecoversBlock ? 0.3 : 0.6); // Recovery

                if (offenseRecoversBlock)
                {
                    rng.NextDouble(0.7);           // Recovery yards
                }
                else
                {
                    rng.NextDouble(0.5)            // Ball bounce base
                        .NextDouble(0.6);          // Ball bounce random
                }

                rng.NextDouble(0.5);               // Elapsed time
                return rng;
            }

            // Normal punt path
            rng.NextDouble(0.99)                   // No block
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time
                .NextDouble(kickerPenalty ? 0.01 : 0.99); // Kicker penalty

            if (kickerPenalty)
            {
                rng.NextDouble(0.5).NextInt(5);    // Penalty effect
            }

            // Touchback - early return
            if (touchback || puntDistance >= 0.9)
            {
                return rng;
            }

            // Out of bounds
            rng.NextDouble(outOfBounds ? 0.1 : 0.8);
            if (outOfBounds)
            {
                return rng;
            }

            // Downed
            rng.NextDouble(downed ? 0.2 : 0.9);
            if (downed)
            {
                return rng;
            }

            // Fair catch
            rng.NextDouble(fairCatch ? 0.2 : 0.9);
            if (fairCatch)
            {
                rng.NextDouble(0.5);               // Elapsed time
                return rng;
            }

            // Muff
            rng.NextDouble(muff ? 0.02 : 0.95);
            if (muff)
            {
                rng.NextDouble(receivingTeamRecoversMuff ? 0.4 : 0.7); // Recovery

                if (receivingTeamRecoversMuff)
                {
                    rng.NextDouble(0.3);           // Recovery yards
                }

                rng.NextDouble(0.5);               // Elapsed time
                return rng;
            }

            // Normal return
            rng.NextDouble(returnYards);           // Return yards

            if (returnYards > 0)
            {
                rng.NextDouble(returnerPenalty ? 0.01 : 0.99); // Returner penalty

                if (returnerPenalty)
                {
                    rng.NextDouble(0.5).NextInt(5); // Penalty effect
                }
            }

            rng.NextDouble(0.5);                   // Elapsed time

            return rng;
        }

        #endregion
    }
}
