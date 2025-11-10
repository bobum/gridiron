using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if a field goal attempt is successful based on kicker skill,
    /// distance, and game conditions.
    /// </summary>
    public class FieldGoalMakeOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _kicker;
        private readonly int _attemptDistance;

        public FieldGoalMakeOccurredSkillsCheck(
            ISeedableRandom rng,
            Player kicker,
            int attemptDistance)
        {
            _rng = rng;
            _kicker = kicker;
            _attemptDistance = attemptDistance;
        }

        public override void Execute(Game game)
        {
            // Calculate base make probability based on distance
            // Short kicks (< 30 yards): 95-99%
            // Medium kicks (30-45 yards): 80-95%
            // Long kicks (45-55 yards): 60-80%
            // Very long kicks (55-65 yards): 30-60%
            // Extremely long (65+ yards): 10-30%

            double baseMakeProbability;

            if (_attemptDistance <= 30)
            {
                // Extra points and very short field goals
                baseMakeProbability = 0.98;
            }
            else if (_attemptDistance <= 40)
            {
                // Routine field goals
                baseMakeProbability = 0.90 - ((_attemptDistance - 30) * 0.01);
            }
            else if (_attemptDistance <= 50)
            {
                // Moderate field goals
                baseMakeProbability = 0.80 - ((_attemptDistance - 40) * 0.015);
            }
            else if (_attemptDistance <= 60)
            {
                // Long field goals
                baseMakeProbability = 0.65 - ((_attemptDistance - 50) * 0.025);
            }
            else
            {
                // Extremely long field goals (60+ yards)
                baseMakeProbability = 0.40 - ((_attemptDistance - 60) * 0.03);
                baseMakeProbability = Math.Max(baseMakeProbability, 0.10); // Minimum 10% for kicks < 70 yards
            }

            // Adjust for kicker skill
            // Kicking skill ranges from 0-100
            // Average kicker (50 skill): no adjustment
            // Excellent kicker (80+ skill): +15% to probability
            // Poor kicker (30 skill): -15% to probability
            var kickerSkillFactor = (_kicker.Kicking - 50) / 200.0; // -0.25 to +0.25
            baseMakeProbability += kickerSkillFactor;

            // Clamp probability between 5% and 99%
            baseMakeProbability = Math.Max(0.05, Math.Min(0.99, baseMakeProbability));

            // Random check
            Occurred = _rng.NextDouble() < baseMakeProbability;
        }
    }
}
