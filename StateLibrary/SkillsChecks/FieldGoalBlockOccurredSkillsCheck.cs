using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
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
            // Base probability by distance (longer kicks = higher trajectory = easier to block)
            double blockProbability;
            if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_SHORT)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_VERY_SHORT;
            else if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_MEDIUM)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_SHORT;
            else if (_kickDistance <= GameProbabilities.FieldGoals.FG_BLOCK_DISTANCE_LONG)
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_MEDIUM;
            else
                blockProbability = GameProbabilities.FieldGoals.FG_BLOCK_LONG;

            // Bad snap multiplier - MUCH easier to block
            if (!_goodSnap)
                blockProbability *= GameProbabilities.FieldGoals.FG_BLOCK_BAD_SNAP_MULTIPLIER;

            // Factor 1: Kicker skill (better kicker = faster operation)
            var kickerSkill = _kicker.Kicking;
            var kickerFactor = 1.0 - (kickerSkill / GameProbabilities.FieldGoals.FG_BLOCK_KICKER_SKILL_DENOMINATOR);
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

                blockProbability += (skillDifferential / 10.0) * GameProbabilities.FieldGoals.FG_BLOCK_DEFENDER_SKILL_FACTOR;
            }

            // Clamp to reasonable range
            blockProbability = Math.Max(
                GameProbabilities.FieldGoals.FG_BLOCK_MIN_CLAMP,
                Math.Min(GameProbabilities.FieldGoals.FG_BLOCK_MAX_CLAMP, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}