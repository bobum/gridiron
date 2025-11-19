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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time random
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.95)
                .PuntReturnYards(returnYards)
                .TacklePenaltyCheck(0.99)
                // Injury checks (returner + 2 tacklers)
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.05)                  // Short punt (easier to return for TD)
                .NextDouble(0.3)                   // Short hang time (poor coverage)
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.95)
                .PuntReturnYards(0.95)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.01)
                .BadSnapYardsBase(baseLoss)
                .BadSnapYardsRandom(randomFactor)
                .ElapsedTimeRandomFactor(0.7);
        }

        /// <summary>
        /// Bad snap resulting in safety - snap into own end zone.
        /// </summary>
        public static TestFluentSeedableRandom BadSnapSafety()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.01)
                .BadSnapYardsBase(0.9)
                .BadSnapYardsRandom(0.8)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.01)
                .BlockedKickRecoveryCheck(0.3)
                .BlockedPuntRecoveryYards(recoveryYards)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.01)
                .BlockedKickRecoveryCheck(0.6)
                .BlockedPuntBounceBase(baseBounce)
                .BlockedPuntBounceRandom(randomFactor)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Blocked punt recovered for touchdown - defense recovers in end zone.
        /// </summary>
        public static TestFluentSeedableRandom BlockedPuntTouchdown()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.01)
                .BlockedKickRecoveryCheck(0.6)
                .BlockedPuntBounceBase(0.7)
                .BlockedPuntBounceRandom(0.5)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Blocked punt recovered by offense in end zone - safety.
        /// </summary>
        public static TestFluentSeedableRandom BlockedPuntSafety()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.01)
                .BlockedKickRecoveryCheck(0.3)
                .BlockedPuntRecoveryYards(0.05)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(puntDistance)          // Very deep punt (into end zone)
                .NextDouble(0.9)                   // Good hang time
                .KickerPenaltyCheck(0.99);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.5)                   // Moderate punt distance (~46 yards, won't touchback from midfield)
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.05);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.7)                   // Good hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.05);
        }

        /// <summary>
        /// Coffin corner punt - downed near goal line (inside 5-yard line).
        /// </summary>
        public static TestFluentSeedableRandom CoffinCorner()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.31)                  // Precise distance to land near goal
                .NextDouble(0.7)                   // Good hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.01);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.9)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.2)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Fair catch deep in own territory - high probability situation.
        /// </summary>
        public static TestFluentSeedableRandom FairCatchDeep()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.63)                  // 52-yard punt lands deep
                .NextDouble(0.9)                   // Good hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.6)
                .PuntFairCatchCheck(0.15)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.35)                  // Punt distance (~45 yards, avoids touchback from midfield)
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.008)
                .MuffRecoveryCheck(0.4)
                .MuffRecoveryYards(recoveryYards)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.35)                  // Punt distance (~45 yards, avoids touchback from midfield)
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.008)
                .MuffRecoveryCheck(0.7)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.95)                  // Touchback distance
                .NextDouble(0.9)                   // Hang time
                .KickerPenaltyCheck(0.01)
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.95)
                .PuntReturnYards(0.5)
                .TacklePenaltyCheck(0.01)
                .NextDouble(0.5)                   // Penalty effect: team selection
                .NextInt(5)                        // Penalty effect: player selection
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
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
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.0)                   // Minimum punt distance (shanked)
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.95)
                .PuntReturnYards(0.5)
                .TacklePenaltyCheck(0.99)
                // Injury checks
                .InjuryOccurredCheck(0.99)
                .TacklerInjuryGateCheck(0.9)
                .TacklerInjuryGateCheck(0.9)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Return with negative yards - returner tackled behind catch point.
        /// </summary>
        public static TestFluentSeedableRandom NegativeReturn()
        {
            return new TestFluentSeedableRandom()
                .BadSnapCheck(0.99)
                .PuntBlockCheck(0.99)
                .NextDouble(0.6)                   // Punt distance
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(0.99)
                .PuntOutOfBoundsCheck(0.8)
                .PuntDownedCheck(0.9)
                .PuntFairCatchCheck(0.9)
                .PuntMuffCheck(0.95)
                .PuntReturnYards(0.0)
                // No injury checks - returnYards = 0 means no tackle happened
                .ElapsedTimeRandomFactor(0.5);
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
                rng.BadSnapCheck(0.01)
                    .BadSnapYardsBase(0.5)
                    .BadSnapYardsRandom(0.5)
                    .ElapsedTimeRandomFactor(0.5);
                return rng;
            }

            // No bad snap
            rng.BadSnapCheck(0.99);

            // Blocked punt path
            if (blocked)
            {
                rng.PuntBlockCheck(0.01)
                    .BlockedKickRecoveryCheck(offenseRecoversBlock ? 0.3 : 0.6);

                if (offenseRecoversBlock)
                {
                    rng.BlockedPuntRecoveryYards(0.7);
                }
                else
                {
                    rng.BlockedPuntBounceBase(0.5)
                        .BlockedPuntBounceRandom(0.6);
                }

                rng.ElapsedTimeRandomFactor(0.5);
                return rng;
            }

            // Normal punt path
            rng.PuntBlockCheck(0.99)
                .NextDouble(puntDistance)          // Punt distance
                .NextDouble(0.5)                   // Hang time
                .KickerPenaltyCheck(kickerPenalty ? 0.01 : 0.99);

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
            rng.PuntOutOfBoundsCheck(outOfBounds ? 0.1 : 0.8);
            if (outOfBounds)
            {
                return rng;
            }

            // Downed
            rng.PuntDownedCheck(downed ? 0.2 : 0.9);
            if (downed)
            {
                return rng;
            }

            // Fair catch
            rng.PuntFairCatchCheck(fairCatch ? 0.2 : 0.9);
            if (fairCatch)
            {
                rng.ElapsedTimeRandomFactor(0.5);
                return rng;
            }

            // Muff
            rng.PuntMuffCheck(muff ? 0.02 : 0.95);
            if (muff)
            {
                rng.MuffRecoveryCheck(receivingTeamRecoversMuff ? 0.4 : 0.7);

                if (receivingTeamRecoversMuff)
                {
                    rng.MuffRecoveryYards(0.3);
                }

                rng.ElapsedTimeRandomFactor(0.5);
                return rng;
            }

            // Normal return
            rng.PuntReturnYards(returnYards);

            if (returnYards > 0)
            {
                rng.TacklePenaltyCheck(returnerPenalty ? 0.01 : 0.99);

                if (returnerPenalty)
                {
                    rng.NextDouble(0.5).NextInt(5); // Penalty effect
                }

                // Injury checks (returner + tacklers) - only if there's a return/tackle
                rng.InjuryOccurredCheck(0.99)
                    .TacklerInjuryGateCheck(0.9)
                    .TacklerInjuryGateCheck(0.9);
            }

            rng.ElapsedTimeRandomFactor(0.5);

            return rng;
        }

        #endregion
    }
}
