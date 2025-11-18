using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a post-play penalty occurred after the play whistle.
    /// Post-play penalties include: Taunting, Unsportsmanlike Conduct,
    /// Personal Foul, Disqualification
    /// </summary>
    public class PostPlayPenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly List<Player> _homePlayersOnField;
        private readonly List<Player> _awayPlayersOnField;
        private readonly bool _bigPlayOccurred;
        private readonly bool _turnoverOccurred;

        /// <summary>
        /// The specific penalty that occurred (if Occurred == true)
        /// </summary>
        public PenaltyNames PenaltyThatOccurred { get; private set; } = PenaltyNames.NoPenalty;

        public PostPlayPenaltyOccurredSkillsCheck(
            ISeedableRandom rng,
            List<Player> homePlayersOnField,
            List<Player> awayPlayersOnField,
            bool bigPlayOccurred = false,
            bool turnoverOccurred = false)
        {
            _rng = rng;
            _homePlayersOnField = homePlayersOnField;
            _awayPlayersOnField = awayPlayersOnField;
            _bigPlayOccurred = bigPlayOccurred;
            _turnoverOccurred = turnoverOccurred;
        }

        public override void Execute(Game game)
        {
            // Post-play penalties
            var eligiblePenalties = new[]
            {
                PenaltyNames.Taunting,               // 0.05%
                PenaltyNames.UnsportsmanlikeConduct, // 0.23%
                PenaltyNames.PersonalFoul,           // 0.02%
                PenaltyNames.Disqualification        // 0.01%
            };

            // Calculate base total probability
            var baseProbability = 0.0;
            foreach (var penaltyName in eligiblePenalties)
            {
                var penalty = PenaltyData.List.Single(p => p.Name == penaltyName);
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
                var penalty = PenaltyData.List.Single(p => p.Name == penaltyName);
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

        private double CalculateContextAdjustedProbability(double baseProbability)
        {
            var adjustmentFactor = 1.0;

            // Big plays lead to more emotion/celebration
            if (_bigPlayOccurred)
            {
                adjustmentFactor *= 2.5; // Much higher chance of taunting/celebration penalty
            }

            // Turnovers lead to more emotion/frustration
            if (_turnoverOccurred)
            {
                adjustmentFactor *= 2.0; // Higher chance of unsportsmanlike conduct
            }

            // Note: Could adjust based on player discipline/morale if we add those properties later
            // For now, use baseline probability

            return baseProbability * adjustmentFactor;
        }
    }
}
