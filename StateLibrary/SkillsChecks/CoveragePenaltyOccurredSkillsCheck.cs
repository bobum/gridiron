using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a coverage penalty occurred during pass coverage.
    /// Coverage penalties include: Defensive Pass Interference, Defensive Holding,
    /// Illegal Contact, Offensive Pass Interference
    /// </summary>
    public class CoveragePenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _receiver;
        private readonly List<Player> _defensiveBacks;
        private readonly bool _passCompleted;
        private readonly int _airYards;

        /// <summary>
        /// The specific penalty that occurred (if Occurred == true)
        /// </summary>
        public PenaltyNames PenaltyThatOccurred { get; private set; } = PenaltyNames.NoPenalty;

        public CoveragePenaltyOccurredSkillsCheck(
            ISeedableRandom rng,
            Player receiver,
            List<Player> defensiveBacks,
            bool passCompleted,
            int airYards)
        {
            _rng = rng;
            _receiver = receiver;
            _defensiveBacks = defensiveBacks;
            _passCompleted = passCompleted;
            _airYards = airYards;
        }

        public override void Execute(Game game)
        {
            // Coverage penalties that can occur during pass plays
            var eligiblePenalties = new[]
            {
                PenaltyNames.DefensivePassInterference, // 0.64%
                PenaltyNames.DefensiveHolding,          // 0.60%
                PenaltyNames.IllegalContact,            // 0.16%
                PenaltyNames.OffensivePassInterference  // 0.27%
            };

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
            // Adjust individual penalty probabilities based on context
            var penaltyProbabilities = CalculateIndividualPenaltyProbabilities(eligiblePenalties);
            var totalAdjustedProb = penaltyProbabilities.Values.Sum();

            var normalizedRoll = roll / adjustedProbability * totalAdjustedProb;
            var cumulativeProb = 0.0;

            foreach (var penaltyName in eligiblePenalties)
            {
                cumulativeProb += penaltyProbabilities[penaltyName];

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

            // Adjust based on defensive back coverage skills
            if (_defensiveBacks.Any())
            {
                var avgCoverage = _defensiveBacks.Average(p => p.Coverage);

                // Worse coverage = more penalties (0.7x to 1.5x)
                adjustmentFactor *= 1.5 - (avgCoverage / 100.0 * 0.8);
            }

            // Adjust based on receiver skills (catching represents overall receiver ability)
            var receiverCatching = _receiver?.Catching ?? 50;

            // Better catching/route skills = more penalties drawn (0.9x to 1.3x)
            adjustmentFactor *= 0.9 + (receiverCatching / 100.0 * 0.4);

            // Deep passes = more DPI (harder to cover)
            if (_airYards > 20)
            {
                adjustmentFactor *= 1.4;
            }
            else if (_airYards > 10)
            {
                adjustmentFactor *= 1.2;
            }

            return baseProbability * adjustmentFactor;
        }

        private Dictionary<PenaltyNames, double> CalculateIndividualPenaltyProbabilities(PenaltyNames[] penalties)
        {
            var probabilities = new Dictionary<PenaltyNames, double>();

            foreach (var penaltyName in penalties)
            {
                var basePenalty = Penalties.List.Single(p => p.Name == penaltyName);
                var baseOdds = basePenalty.Odds;

                // Context-specific adjustments
                if (penaltyName == PenaltyNames.DefensivePassInterference)
                {
                    // DPI more likely on incomplete deep passes
                    if (!_passCompleted && _airYards > 15)
                    {
                        baseOdds *= 2.5;
                    }
                    else if (_passCompleted)
                    {
                        // Rare on completions (would be defensive holding instead)
                        baseOdds *= 0.1;
                    }
                }
                else if (penaltyName == PenaltyNames.DefensiveHolding)
                {
                    // More common on short/intermediate routes
                    if (_airYards < 10)
                    {
                        baseOdds *= 1.5;
                    }
                }
                else if (penaltyName == PenaltyNames.IllegalContact)
                {
                    // Occurs within 5 yards of line (short passes)
                    if (_airYards < 5)
                    {
                        baseOdds *= 2.0;
                    }
                    else
                    {
                        baseOdds *= 0.3;
                    }
                }
                else if (penaltyName == PenaltyNames.OffensivePassInterference)
                {
                    // Pick plays, push-offs - more on intermediate routes
                    if (_airYards >= 5 && _airYards <= 15)
                    {
                        baseOdds *= 1.3;
                    }
                }

                probabilities[penaltyName] = baseOdds;
            }

            return probabilities;
        }
    }
}
