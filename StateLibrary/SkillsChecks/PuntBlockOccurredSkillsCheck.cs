using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    public class PuntBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _punter;
        private readonly List<Player> _offensiveLine;
        private readonly List<Player> _defensiveRushers;
        private readonly bool _goodSnap;

        public PuntBlockOccurredSkillsCheck(
            ISeedableRandom rng,
            Player punter,
            List<Player> offensiveLine,
            List<Player> defensiveRushers,
            bool goodSnap)
        {
            _rng = rng;
            _punter = punter;
            _offensiveLine = offensiveLine;
            _defensiveRushers = defensiveRushers;
            _goodSnap = goodSnap;
        }

        public override void Execute(Game game)
        {
            // Base block probability
            var blockProbability = _goodSnap
                ? GameProbabilities.Punts.PUNT_BLOCK_GOOD_SNAP
                : GameProbabilities.Punts.PUNT_BLOCK_BAD_SNAP;

            // Factor 1: Punter skill (better punter = faster release)
            var punterSkill = _punter.Kicking;
            var punterFactor = 1.0 - (punterSkill / GameProbabilities.Punts.PUNT_BLOCK_PUNTER_SKILL_DENOMINATOR);
            blockProbability *= punterFactor;

            // Factor 2: Best rusher vs average blocker
            var bestRusher = _defensiveRushers
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (_offensiveLine.Count > 0 && bestRusher != null)
            {
                var avgBlocker = _offensiveLine.Average(p => p.Strength + p.Awareness);
                var rusherSkill = (bestRusher.Strength + bestRusher.Speed) / 2.0;
                var skillDifferential = rusherSkill - (avgBlocker / 2.0);

                blockProbability += (skillDifferential / 10.0) * GameProbabilities.Punts.PUNT_BLOCK_DEFENDER_SKILL_FACTOR;
            }

            // Clamp to reasonable range
            blockProbability = Math.Max(
                GameProbabilities.Punts.PUNT_BLOCK_MIN_CLAMP,
                Math.Min(GameProbabilities.Punts.PUNT_BLOCK_MAX_CLAMP, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}