using System;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Factory methods for creating TestFluentSeedableRandom objects configured for kickoff play test scenarios.
    ///
    /// PURPOSE: Centralize random value sequences for kickoff plays. Kickoffs have multiple execution paths
    /// (touchback, normal return, onside kick, out of bounds, touchdown) with different random value sequences.
    ///
    /// RANDOM SEQUENCE ORDER (current as of Phase 2):
    ///
    /// IMPORTANT: Onside kick decision check is CONDITIONAL - only consumed if team is trailing
    /// by 7+ points (see Kickoff.cs line 66). Most normal kickoffs don't consume this random value.
    ///
    /// NORMAL RETURN PATH:
    /// 1. Kick distance (NextDouble 0-1) - determines how far kick travels
    /// 2. Out of bounds check (NextDouble) - kick goes OOB if < 0.03-0.10 depending on landing spot
    /// 3. Muff check (NextDouble) - returner muffs if < muff threshold
    /// 4. Fair catch check (NextDouble) - fair catch if < 0.25-0.55 (varies by situation)
    /// 5. Return yardage (NextDouble 0-1) - return distance calculation
    /// 6. Blocking penalty check (NextDouble)
    /// 7. Fumble check (NextDouble)
    /// 8. Tackle penalty check (NextDouble) - only if not TD
    /// 9. Elapsed time (NextDouble) - only for normal returns, NOT for fair catch/muff
    ///
    /// TOUCHBACK PATH (kick distance >= threshold for endzone):
    /// 1. Kick distance (very high like 0.9+)
    /// 2. Out of bounds check (NextDouble)
    /// (Elapsed time is CONSTANT 3.0 seconds, not random)
    ///
    /// OUT OF BOUNDS PATH:
    /// 1. Kick distance (0.3-0.6 range for danger zone 65-95 yards)
    /// 2. Out of bounds check (< 0.10, triggers OOB)
    /// (No additional randoms - ball placed at 40, elapsed time is constant)
    ///
    /// ONSIDE KICK PATH (requires trailing by 7+ points):
    /// 1. Onside decision (< 0.05, only checked if trailing)
    /// 2. Onside kick distance (0-1 for 10-15 yard range)
    /// 3. Recovery check (< 0.20-0.30 = kicking team recovers)
    /// 4. Elapsed time (NextDouble)
    ///
    /// RETURN TOUCHDOWN PATH:
    /// Same as normal return but:
    /// - High return yardage (0.9+)
    /// - NO tackle penalty check (TD ends play early)
    /// - Elapsed time is still consumed (NextDouble)
    /// </summary>
    public static class KickoffPlayScenarios
    {
        #region Normal Kickoff Scenarios

        /// <summary>
        /// Normal kickoff with return - standard kickoff with moderate return.
        /// Most common kickoff scenario.
        ///
        /// Random sequence: Kick distance → OOB check → Muff → Fair catch → Return yards →
        /// Blocking penalty → Fumble → Tackle penalty → Elapsed time
        /// </summary>
        /// <param name="kickDistance">Kick distance value (0-1), 0.5 = moderate ~70 yards</param>
        /// <param name="returnYardage">Return yardage value (0-1), 0.3 = short return</param>
        public static TestFluentSeedableRandom NormalReturn(double kickDistance = 0.5, double returnYardage = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(kickDistance)          // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.9)
                .KickoffReturnYards(returnYardage)
                .BlockingPenaltyCheck(0.99)
                .FumbleCheck(0.99)
                .TacklePenaltyCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Short kickoff with limited return - weak kicker or intentional short kick.
        /// </summary>
        public static TestFluentSeedableRandom ShortKick(double returnYardage = 0.3)
        {
            return NormalReturn(kickDistance: 0.1, returnYardage: returnYardage);
        }

        /// <summary>
        /// Deep kickoff with return - strong kicker, good field position for return team.
        /// </summary>
        public static TestFluentSeedableRandom DeepKick(double returnYardage = 0.5)
        {
            return NormalReturn(kickDistance: 0.8, returnYardage: returnYardage);
        }

        #endregion

        #region Touchback Scenarios

        /// <summary>
        /// Touchback - kick into end zone, ball placed at 25-yard line.
        ///
        /// Random sequence: Kick distance (very high) → Out of bounds check
        /// NOTE: Much shorter sequence than normal return - only 2 random values.
        /// Elapsed time is set to constant (3.0 seconds), not random.
        /// </summary>
        /// <param name="kickDistance">Kick distance (0.9+ for touchback)</param>
        public static TestFluentSeedableRandom Touchback(double kickDistance = 0.95)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(kickDistance)          // Very deep kick (into end zone)
                .KickoffOutOfBoundsCheck(0.5);
        }

        #endregion

        #region Out of Bounds Scenarios

        /// <summary>
        /// Out of bounds kick - kick goes out of bounds, penalty to 40-yard line.
        ///
        /// Random sequence: Kick distance (danger zone 0.3-0.6) → OOB check (< 0.10)
        /// NOTE: Short sequence - ball placed at 40, no return.
        /// </summary>
        public static TestFluentSeedableRandom OutOfBounds()
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.3)                   // Kick distance in danger zone
                .KickoffOutOfBoundsCheck(0.05);
        }

        #endregion

        #region Onside Kick Scenarios

        /// <summary>
        /// Onside kick recovered by kicking team - successful onside kick.
        ///
        /// Random sequence: Onside decision → Onside distance → Recovery check (succeeds) → Elapsed time
        /// </summary>
        /// <param name="onsideDistance">Onside kick distance (0-1 for 10-15 yards)</param>
        public static TestFluentSeedableRandom OnsideKickRecovered(double onsideDistance = 0.3)
        {
            return new TestFluentSeedableRandom()
                .OnsideKickDecisionCheck(0.01)
                .OnsideKickDistance(onsideDistance)
                .OnsideKickRecoveryCheck(0.15)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Onside kick recovered by receiving team - failed onside kick attempt.
        ///
        /// Random sequence: Onside decision → Onside distance → Recovery check (fails) → Elapsed time
        /// </summary>
        /// <param name="onsideDistance">Onside kick distance (0-1 for 10-15 yards)</param>
        public static TestFluentSeedableRandom OnsideKickNotRecovered(double onsideDistance = 0.4)
        {
            return new TestFluentSeedableRandom()
                .OnsideKickDecisionCheck(0.02)
                .OnsideKickDistance(onsideDistance)
                .OnsideKickRecoveryCheck(0.85)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Onside kick at minimum distance (10 yards exactly).
        /// </summary>
        public static TestFluentSeedableRandom OnsideKickMinimumDistance(bool kickingTeamRecovers = true)
        {
            return new TestFluentSeedableRandom()
                .OnsideKickDecisionCheck(0.01)
                .OnsideKickDistance(0.0)
                .OnsideKickRecoveryCheck(kickingTeamRecovers ? 0.25 : 0.85)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Fair Catch Scenarios

        /// <summary>
        /// Fair catch - returner signals fair catch, no return.
        ///
        /// Random sequence: Kick distance → OOB check → Muff → Fair catch (succeeds) → Elapsed time
        /// NOTE: No return yardage, fumble, or penalty checks after fair catch.
        /// </summary>
        public static TestFluentSeedableRandom FairCatch(double kickDistance = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(kickDistance)          // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.3)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Muff Scenarios

        /// <summary>
        /// Muffed kick - returner muffs the catch, ball becomes live.
        ///
        /// Random sequence: Kick distance → OOB → Muff (succeeds) → Recovery → Elapsed time
        /// </summary>
        /// <param name="kickingTeamRecovers">Whether kicking team recovers the muff</param>
        public static TestFluentSeedableRandom Muff(bool kickingTeamRecovers = false)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.5)                   // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.01)
                .MuffRecoveryCheck(kickingTeamRecovers ? 0.3 : 0.7)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Return Touchdown Scenarios

        /// <summary>
        /// Return touchdown - returner returns kick all the way for TD.
        ///
        /// Random sequence: Kick distance → OOB → Muff → Fair catch → Return yards (very high) →
        /// Blocking penalty → Fumble → Elapsed time
        ///
        /// NOTE: NO tackle penalty check (TD ends play early, similar to pass play TDs).
        /// </summary>
        /// <param name="kickDistance">Kick distance (short kicks easier to return for TD)</param>
        public static TestFluentSeedableRandom ReturnTouchdown(double kickDistance = 0.2)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(kickDistance)          // Kick distance (short = easier TD)
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.9)
                .KickoffReturnYards(0.95)
                .BlockingPenaltyCheck(0.99)
                .FumbleCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Maximum return touchdown - worst case for kicking team.
        /// Very short kick with maximum return.
        /// </summary>
        public static TestFluentSeedableRandom ReturnTouchdownMaximum()
        {
            return ReturnTouchdown(kickDistance: 0.1);
        }

        #endregion

        #region Fumble Scenarios

        /// <summary>
        /// Fumble during return - returner fumbles during kickoff return.
        ///
        /// Random sequence: Same as normal return but fumble check = 0.01.
        /// </summary>
        /// <param name="kickingTeamRecovers">Whether kicking team recovers the fumble</param>
        public static TestFluentSeedableRandom FumbleOnReturn(bool kickingTeamRecovers = false, double returnYardage = 0.4)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.5)                   // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.9)
                .KickoffReturnYards(returnYardage)
                .BlockingPenaltyCheck(0.99)
                .FumbleCheck(0.01)
                .TacklePenaltyCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Penalty Scenarios

        /// <summary>
        /// Blocking penalty during return - illegal block on kickoff return.
        ///
        /// Random sequence: Same as normal return but blocking penalty = 0.01.
        /// </summary>
        public static TestFluentSeedableRandom WithBlockingPenalty(double returnYardage = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.5)                   // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.9)
                .KickoffReturnYards(returnYardage)
                .BlockingPenaltyCheck(0.01)
                .FumbleCheck(0.99)
                .TacklePenaltyCheck(0.99)
                .ElapsedTimeRandomFactor(0.5);
        }

        /// <summary>
        /// Tackle penalty during return - unnecessary roughness on tackle.
        ///
        /// Random sequence: Same as normal return but tackle penalty = 0.01.
        /// </summary>
        public static TestFluentSeedableRandom WithTacklePenalty(double returnYardage = 0.5)
        {
            return new TestFluentSeedableRandom()
                .NextDouble(0.5)                   // Kick distance
                .KickoffOutOfBoundsCheck(0.5)
                .KickoffMuffCheck(0.99)
                .KickoffFairCatchCheck(0.9)
                .KickoffReturnYards(returnYardage)
                .BlockingPenaltyCheck(0.99)
                .FumbleCheck(0.99)
                .TacklePenaltyCheck(0.01)
                .ElapsedTimeRandomFactor(0.5);
        }

        #endregion

        #region Custom Builders

        /// <summary>
        /// Custom scenario builder for complete control over all random values.
        /// Use this for edge cases or when testing specific combinations not covered by other scenarios.
        ///
        /// IMPORTANT: When random sequences change, custom builders will need manual updates.
        /// Prefer using the named scenario methods above for maintainability.
        /// </summary>
        public static TestFluentSeedableRandom Custom(
            double kickDistance = 0.5,
            bool outOfBounds = false,
            bool muff = false,
            bool fairCatch = false,
            double returnYardage = 0.5,
            bool blockingPenalty = false,
            bool fumble = false,
            bool tacklePenalty = false,
            bool isOnside = false,
            bool onsideRecovered = false)
        {
            var rng = new TestFluentSeedableRandom();

            if (isOnside)
            {
                // Onside kick path
                rng.OnsideKickDecisionCheck(0.01)
                    .OnsideKickDistance(kickDistance)
                    .OnsideKickRecoveryCheck(onsideRecovered ? 0.15 : 0.85)
                    .ElapsedTimeRandomFactor(0.5);
            }
            else
            {
                // Normal kick path
                rng.NextDouble(kickDistance);                       // Kick distance

                // Check if this will be a touchback (simplified check)
                if (kickDistance >= 0.9)
                {
                    // Touchback - short sequence
                    rng.ElapsedTimeRandomFactor(0.5);
                }
                else
                {
                    // Normal return path
                    rng.KickoffOutOfBoundsCheck(outOfBounds ? 0.05 : 0.5);

                    if (!outOfBounds)
                    {
                        rng.KickoffMuffCheck(muff ? 0.01 : 0.99);

                        if (!muff)
                        {
                            rng.KickoffFairCatchCheck(fairCatch ? 0.3 : 0.9);

                            if (!fairCatch)
                            {
                                // Full return sequence
                                rng.KickoffReturnYards(returnYardage)
                                    .BlockingPenaltyCheck(blockingPenalty ? 0.01 : 0.99)
                                    .FumbleCheck(fumble ? 0.01 : 0.99);

                                // Only add tackle penalty if not TD (simplified)
                                if (returnYardage < 0.9)
                                {
                                    rng.TacklePenaltyCheck(tacklePenalty ? 0.01 : 0.99);
                                }
                            }
                        }
                        else
                        {
                            rng.MuffRecoveryCheck(0.7);
                        }

                        rng.ElapsedTimeRandomFactor(0.5);
                    }
                }
            }

            return rng;
        }

        #endregion
    }
}
