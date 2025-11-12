using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    public class FieldGoalBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _kicker;
        private readonly int _kickDistance;
        private readonly List<Player> _offensiveLine;
        private readonly List<Player> _defensiveRushers;
        private readonly bool _goodSnap;

        public FieldGoalBlockOccurredSkillsCheck(
            ISeedableRandom rng,
            Player kicker,
            int kickDistance,
            List<Player> offensiveLine,
            List<Player> defensiveRushers,
            bool goodSnap)
        {
            _rng = rng;
            _kicker = kicker;
            _kickDistance = kickDistance;
            _offensiveLine = offensiveLine;
            _defensiveRushers = defensiveRushers;
            _goodSnap = goodSnap;
        }

        public override void Execute(Game game)
        {
            // Base probability by distance
            // Longer kicks = higher trajectory = easier to block
            double blockProbability;
            if (_kickDistance <= 30)
                blockProbability = 0.015; // 1.5% for short kicks/extra points (18-30 yards)
            else if (_kickDistance <= 45)
                blockProbability = 0.025; // 2.5% for medium kicks (30-45 yards)
            else if (_kickDistance <= 55)
                blockProbability = 0.040; // 4% for long kicks (45-55 yards)
            else
                blockProbability = 0.065; // 6.5% for 55+ yard kicks (very long)

            // Bad snap multiplier - MUCH easier to block
            if (!_goodSnap)
                blockProbability *= 10.0; // 10x multiplier for bad snap

            // Factor 1: Kicker skill (better kicker = faster operation)
            var kickerSkill = _kicker.Kicking;
            var kickerFactor = 1.0 - (kickerSkill / 300.0); // 0.67 to 1.0 multiplier
            blockProbability *= kickerFactor;

            // Factor 2: Defensive pressure (best rusher vs avg blocker)
            var bestRusher = _defensiveRushers
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (_offensiveLine.Count > 0 && bestRusher != null)
            {
                var avgBlocker = _offensiveLine.Average(p => p.Strength + p.Awareness);
                var rusherSkill = (bestRusher.Strength + bestRusher.Speed) / 2.0;
                var skillDifferential = rusherSkill - (avgBlocker / 2.0);

                // +/- 0.003 per 10 point skill differential
                blockProbability += (skillDifferential / 10.0) * 0.003;
            }

            // Clamp to reasonable range
            // Minimum: 0.5% (short kick, elite kicker, great protection)
            // Maximum: 25% (bad snap on long kick with poor protection)
            blockProbability = Math.Max(0.005, Math.Min(0.25, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}