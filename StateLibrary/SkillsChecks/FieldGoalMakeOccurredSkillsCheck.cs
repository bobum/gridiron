using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
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

            if (_attemptDistance <= GameProbabilities.FieldGoals.FG_DISTANCE_SHORT)
            {
                // Extra points and very short field goals
                baseMakeProbability = GameProbabilities.FieldGoals.FG_MAKE_VERY_SHORT;
            }
            else if (_attemptDistance <= GameProbabilities.FieldGoals.FG_DISTANCE_MEDIUM)
            {
                // Routine field goals
                baseMakeProbability = GameProbabilities.FieldGoals.FG_MAKE_SHORT_BASE
                    - ((_attemptDistance - GameProbabilities.FieldGoals.FG_DISTANCE_SHORT)
                        * GameProbabilities.FieldGoals.FG_MAKE_SHORT_DECAY);
            }
            else if (_attemptDistance <= GameProbabilities.FieldGoals.FG_DISTANCE_LONG)
            {
                // Moderate field goals
                baseMakeProbability = GameProbabilities.FieldGoals.FG_MAKE_MEDIUM_BASE
                    - ((_attemptDistance - GameProbabilities.FieldGoals.FG_DISTANCE_MEDIUM)
                        * GameProbabilities.FieldGoals.FG_MAKE_MEDIUM_DECAY);
            }
            else if (_attemptDistance <= GameProbabilities.FieldGoals.FG_DISTANCE_VERY_LONG)
            {
                // Long field goals
                baseMakeProbability = GameProbabilities.FieldGoals.FG_MAKE_LONG_BASE
                    - ((_attemptDistance - GameProbabilities.FieldGoals.FG_DISTANCE_LONG)
                        * GameProbabilities.FieldGoals.FG_MAKE_LONG_DECAY);
            }
            else
            {
                // Extremely long field goals (60+ yards)
                baseMakeProbability = GameProbabilities.FieldGoals.FG_MAKE_VERY_LONG_BASE
                    - ((_attemptDistance - GameProbabilities.FieldGoals.FG_DISTANCE_VERY_LONG)
                        * GameProbabilities.FieldGoals.FG_MAKE_VERY_LONG_DECAY);
                baseMakeProbability = Math.Max(baseMakeProbability, GameProbabilities.FieldGoals.FG_MAKE_MIN_CLAMP);
            }

            // Adjust for kicker skill
            var kickerSkillFactor = (_kicker.Kicking - 50) / GameProbabilities.FieldGoals.FG_MAKE_SKILL_DENOMINATOR;
            baseMakeProbability += kickerSkillFactor;

            // Clamp probability to reasonable bounds
            baseMakeProbability = Math.Max(
                GameProbabilities.FieldGoals.FG_MAKE_MIN_CLAMP,
                Math.Min(GameProbabilities.FieldGoals.FG_MAKE_MAX_CLAMP, baseMakeProbability));

            // Random check
            Occurred = _rng.NextDouble() < baseMakeProbability;
        }
    }
}
