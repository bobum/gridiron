using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a tackle/contact penalty occurred during or after a tackle.
    /// Tackle penalties include: Unnecessary Roughness, Facemask, Roughing the Passer,
    /// Horse Collar Tackle, Personal Foul, Roughing the Kicker, Running into the Kicker
    /// </summary>
    public class TacklePenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _ballCarrier;
        private readonly List<Player> _tacklers;
        private readonly TackleContext _tackleContext;

        /// <summary>
        /// The specific penalty that occurred (if Occurred == true)
        /// </summary>
        public PenaltyNames PenaltyThatOccurred { get; private set; } = PenaltyNames.NoPenalty;

        public TacklePenaltyOccurredSkillsCheck(
            ISeedableRandom rng,
            Player ballCarrier,
            List<Player> tacklers,
            TackleContext tackleContext)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
            _tacklers = tacklers;
            _tackleContext = tackleContext;
        }

        public override void Execute(Game game)
        {
            // Determine eligible penalties based on tackle context
            var eligiblePenalties = GetEligiblePenaltiesForContext();

            if (!eligiblePenalties.Any())
            {
                Occurred = false;
                PenaltyThatOccurred = PenaltyNames.NoPenalty;
                return;
            }

            // Calculate base total probability
            var baseProbability = 0.0;
            foreach (var penaltyName in eligiblePenalties)
            {
                var penalty = Penalties.List.Single(p => p.Name == penaltyName);
                baseProbability += penalty.Odds;
            }

            // Adjust probability based on context
            var adjustedProbability = CalculateContextAdjustedProbability(baseProbability);

            // Check if any penalty occurs
            var roll = _rng.NextDouble();

            if (roll >= adjustedProbability)
            {
                // No penalty occurred
                Occurred = false;
                PenaltyThatOccurred = PenaltyNames.NoPenalty;
                Margin = roll - adjustedProbability;
                return;
            }

            // A penalty occurred - determine which one
            var normalizedRoll = roll / adjustedProbability * baseProbability;
            var cumulativeProb = 0.0;

            foreach (var penaltyName in eligiblePenalties)
            {
                var penalty = Penalties.List.Single(p => p.Name == penaltyName);
                cumulativeProb += penalty.Odds;

                if (normalizedRoll < cumulativeProb)
                {
                    Occurred = true;
                    PenaltyThatOccurred = penaltyName;
                    Margin = cumulativeProb - normalizedRoll;
                    return;
                }
            }

            // Fallback
            Occurred = false;
            PenaltyThatOccurred = PenaltyNames.NoPenalty;
        }

        private List<PenaltyNames> GetEligiblePenaltiesForContext()
        {
            var penalties = new List<PenaltyNames>();

            switch (_tackleContext)
            {
                case TackleContext.PasserInPocket:
                case TackleContext.PasserScrambling:
                    penalties.Add(PenaltyNames.RoughingthePasser);    // 0.27%
                    penalties.Add(PenaltyNames.UnnecessaryRoughness); // 0.62%
                    penalties.Add(PenaltyNames.FaceMask15Yards);      // 0.27%
                    penalties.Add(PenaltyNames.PersonalFoul);         // 0.02%
                    break;

                case TackleContext.Kicker:
                    penalties.Add(PenaltyNames.RoughingtheKicker);    // 0.01%
                    penalties.Add(PenaltyNames.RunningIntotheKicker); // 0.03%
                    break;

                case TackleContext.BallCarrier:
                case TackleContext.Receiver:
                    penalties.Add(PenaltyNames.UnnecessaryRoughness); // 0.62%
                    penalties.Add(PenaltyNames.FaceMask15Yards);      // 0.27%
                    penalties.Add(PenaltyNames.HorseCollarTackle);    // 0.04%
                    penalties.Add(PenaltyNames.PersonalFoul);         // 0.02%
                    penalties.Add(PenaltyNames.Tripping);             // 0.03%
                    break;

                case TackleContext.Returner:
                    penalties.Add(PenaltyNames.UnnecessaryRoughness); // 0.62%
                    penalties.Add(PenaltyNames.FaceMask15Yards);      // 0.27%
                    penalties.Add(PenaltyNames.HorseCollarTackle);    // 0.04%
                    penalties.Add(PenaltyNames.PersonalFoul);         // 0.02%
                    break;
            }

            return penalties;
        }

        private double CalculateContextAdjustedProbability(double baseProbability)
        {
            var adjustmentFactor = 1.0;

            // Adjust based on tackler strength (stronger = more penalties due to harder hits)
            if (_tacklers.Any())
            {
                var avgTacklerStrength = _tacklers.Average(p => p.Strength);

                // Stronger tacklers = slightly more penalties (0.9x to 1.2x)
                adjustmentFactor *= 0.9 + (avgTacklerStrength / 100.0 * 0.3);
            }

            // Context-specific adjustments
            switch (_tackleContext)
            {
                case TackleContext.PasserInPocket:
                    // More scrutiny on QB hits
                    adjustmentFactor *= 1.5;
                    break;

                case TackleContext.PasserScrambling:
                    // Slightly less scrutiny when QB is runner
                    adjustmentFactor *= 1.2;
                    break;

                case TackleContext.Kicker:
                    // Very high penalty rate on kicker contact
                    adjustmentFactor *= 3.0;
                    break;

                case TackleContext.BallCarrier:
                    // Normal penalty rate
                    adjustmentFactor *= 1.0;
                    break;

                case TackleContext.Receiver:
                    // Slightly higher (defenseless receiver)
                    adjustmentFactor *= 1.3;
                    break;

                case TackleContext.Returner:
                    // Normal penalty rate
                    adjustmentFactor *= 1.0;
                    break;
            }

            return baseProbability * adjustmentFactor;
        }
    }

    /// <summary>
    /// Context for when a tackle occurs, affects which penalties are possible
    /// </summary>
    public enum TackleContext
    {
        /// <summary>
        /// Quarterback in the pocket being sacked
        /// </summary>
        PasserInPocket,

        /// <summary>
        /// Quarterback scrambling outside the pocket
        /// </summary>
        PasserScrambling,

        /// <summary>
        /// Punter or field goal kicker
        /// </summary>
        Kicker,

        /// <summary>
        /// Ball carrier on a run play
        /// </summary>
        BallCarrier,

        /// <summary>
        /// Receiver being tackled after catch
        /// </summary>
        Receiver,

        /// <summary>
        /// Returner on kickoff/punt
        /// </summary>
        Returner
    }
}
