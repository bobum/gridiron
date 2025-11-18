using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a blocking penalty occurred during run or pass protection.
    /// Blocking penalties include: Offensive Holding, Illegal Use of Hands,
    /// Illegal Block Above the Waist, Clipping, Chop Block, Low Block, Illegal Peelback, Illegal Crackback
    /// </summary>
    public class BlockingPenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly List<Player> _offensiveLinemen;
        private readonly List<Player> _defensivePlayers;
        private readonly PlayType _playType;

        /// <summary>
        /// The specific penalty that occurred (if Occurred == true)
        /// </summary>
        public PenaltyNames PenaltyThatOccurred { get; private set; } = PenaltyNames.NoPenalty;

        public BlockingPenaltyOccurredSkillsCheck(
            ISeedableRandom rng,
            List<Player> offensiveLinemen,
            List<Player> defensivePlayers,
            PlayType playType)
        {
            _rng = rng;
            _offensiveLinemen = offensiveLinemen;
            _defensivePlayers = defensivePlayers;
            _playType = playType;
        }

        public override void Execute(Game game)
        {
            // Blocking penalties that can occur during blocking
            var eligiblePenalties = new[]
            {
                PenaltyNames.OffensiveHolding,          // 1.90% - most common
                PenaltyNames.IllegalUseofHands,         // 0.31%
                PenaltyNames.IllegalBlockAbovetheWaist, // 0.34%
                PenaltyNames.Clipping,                  // 0.02%
                PenaltyNames.ChopBlock,                 // 0.04%
                PenaltyNames.LowBlock,                  // 0.01%
                PenaltyNames.IllegalPeelback,           // 0.01%
                PenaltyNames.IllegalCrackback           // 0.01%
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

            // A penalty occurred - determine which one using weighted random selection
            var normalizedRoll = roll / adjustedProbability * baseProbability; // Scale roll back to base probability range
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

            // Adjust based on offensive line skill
            if (_offensiveLinemen.Any())
            {
                var avgOLineSkill = _offensiveLinemen.Average(p => p.Blocking);

                // Better O-line = fewer holding penalties (0.7x to 1.3x)
                adjustmentFactor *= 1.3 - (avgOLineSkill / 100.0 * 0.6);
            }

            // Adjust based on defensive pressure
            if (_defensivePlayers.Any())
            {
                var avgDLineSkill = _defensivePlayers
                    .Where(p => p.Position == Positions.DE || p.Position == Positions.DT)
                    .DefaultIfEmpty()
                    .Average(p => p?.Tackling ?? 50);

                // Better tackling = more holding (0.8x to 1.4x)
                adjustmentFactor *= 0.8 + (avgDLineSkill / 100.0 * 0.6);
            }

            // Play type adjustment
            if (_playType == PlayType.Pass)
            {
                // More holding on pass plays
                adjustmentFactor *= 1.2;
            }
            else if (_playType == PlayType.Run)
            {
                // Slightly more illegal blocks on run plays
                adjustmentFactor *= 1.1;
            }

            return baseProbability * adjustmentFactor;
        }
    }
}
