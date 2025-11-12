using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
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
            // Good snap: ~1% (0.01)
            // Bad snap: ~20% (0.20) - defender has much more time
            var blockProbability = _goodSnap ? 0.01 : 0.20;

            // Factor 1: Punter skill (better punter = faster release)
            // Kicking skill represents how quickly they get the ball away
            var punterSkill = _punter.Kicking; // 0-100
            var punterFactor = 1.0 - (punterSkill / 200.0); // 0.5 to 1.0 multiplier
            blockProbability *= punterFactor;

            // Factor 2: Best rusher vs average blocker
            // Find the best defensive rusher
            var bestRusher = _defensiveRushers
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (_offensiveLine.Count > 0 && bestRusher != null)
            {
                var avgBlocker = _offensiveLine.Average(p => p.Strength + p.Awareness);
                var rusherSkill = (bestRusher.Strength + bestRusher.Speed) / 2.0;
                var skillDifferential = rusherSkill - (avgBlocker / 2.0);

                // +/- 0.005 per 10 point skill differential
                // This means a +20 skill advantage adds ~1% block chance
                blockProbability += (skillDifferential / 10.0) * 0.005;
            }

            // Clamp to reasonable range
            // Minimum: 0.2% (elite punter, great protection)
            // Maximum: 30% (bad snap + poor protection)
            blockProbability = Math.Max(0.002, Math.Min(0.30, blockProbability));

            Occurred = _rng.NextDouble() < blockProbability;
        }
    }
}