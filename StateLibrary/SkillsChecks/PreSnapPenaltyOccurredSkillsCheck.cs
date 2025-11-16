using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a pre-snap penalty occurred.
    /// Pre-snap penalties include: False Start, Delay of Game, Illegal Formation,
    /// Encroachment, Defensive Offside, Neutral Zone Infraction, Illegal Shift, Illegal Motion,
    /// 12 Men on Field, Illegal Substitution, Offensive Offside
    /// </summary>
    public class PreSnapPenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly PlayType _playType;

        /// <summary>
        /// The specific penalty that occurred (if Occurred == true)
        /// </summary>
        public PenaltyNames PenaltyThatOccurred { get; private set; } = PenaltyNames.NoPenalty;

        public PreSnapPenaltyOccurredSkillsCheck(ISeedableRandom rng, PlayType playType)
        {
            _rng = rng;
            _playType = playType;
        }

        public override void Execute(Game game)
        {
            // Pre-snap penalties that can occur on any play type
            var eligiblePenalties = new[]
            {
                PenaltyNames.FalseStart,            // 1.55%
                PenaltyNames.DelayofGame,           // 0.40%
                PenaltyNames.DefensiveOffside,      // 0.47%
                PenaltyNames.NeutralZoneInfraction, // 0.42%
                PenaltyNames.IllegalFormation,      // 0.16%
                PenaltyNames.Encroachment,          // 0.12%
                PenaltyNames.IllegalShift,          // 0.08%
                PenaltyNames.IllegalMotion,         // 0.03%
                PenaltyNames.Offensive12OnField,    // 0.02%
                PenaltyNames.Defensive12OnField,    // 0.14%
                PenaltyNames.IllegalSubstitution,   // 0.02%
                PenaltyNames.OffensiveOffside       // 0.01%
            };

            // Calculate total probability - sum of all penalty odds
            var totalProbability = 0.0;
            foreach (var penaltyName in eligiblePenalties)
            {
                var penalty = Penalties.List.Single(p => p.Name == penaltyName);
                totalProbability += penalty.Odds;
            }

            // Check if any penalty occurs
            var roll = _rng.NextDouble();

            if (roll >= totalProbability)
            {
                // No penalty occurred
                Occurred = false;
                PenaltyThatOccurred = PenaltyNames.NoPenalty;
                Margin = roll - totalProbability; // How close to a penalty
                return;
            }

            // A penalty occurred - determine which one
            // Use weighted random selection based on individual penalty odds
            var cumulativeProb = 0.0;
            foreach (var penaltyName in eligiblePenalties)
            {
                var penalty = Penalties.List.Single(p => p.Name == penaltyName);
                cumulativeProb += penalty.Odds;

                if (roll < cumulativeProb)
                {
                    Occurred = true;
                    PenaltyThatOccurred = penaltyName;
                    Margin = cumulativeProb - roll; // How far into the penalty range
                    return;
                }
            }

            // Fallback (should never reach here)
            Occurred = false;
            PenaltyThatOccurred = PenaltyNames.NoPenalty;
        }
    }
}
