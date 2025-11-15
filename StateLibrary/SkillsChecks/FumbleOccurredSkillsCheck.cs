using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsChecks
{
    public class FumbleOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _ballCarrier;
        private readonly List<Player> _defenders;
        private readonly PlayType _playType;
        private readonly bool _isQBSack;

        public FumbleOccurredSkillsCheck(
            ISeedableRandom rng,
            Player ballCarrier,
            List<Player> defenders,
            PlayType playType,
            bool isQBSack = false)
        {
            _rng = rng;
            _ballCarrier = ballCarrier;
            _defenders = defenders;
            _playType = playType;
            _isQBSack = isQBSack;
        }

        public override void Execute(Game game)
        {
            double fumbleProbability;

            // Base probability by play type
            if (_isQBSack)
            {
                fumbleProbability = 0.12; // 12% for QB sacks
            }
            else if (_playType == PlayType.Kickoff || _playType == PlayType.Punt)
            {
                fumbleProbability = 0.025; // 2.5% for returns
            }
            else
            {
                fumbleProbability = 0.015; // 1.5% for normal runs/catches
            }

            // Ball carrier security factor
            // Use Awareness as proxy for ball security (higher = better)
            var carrierSecurity = _ballCarrier.Awareness; // 0-100
            var securityFactor = 1.0 - (carrierSecurity / 200.0); // 0.5 to 1.0 multiplier
            fumbleProbability *= securityFactor;

            // Defensive pressure factor
            // Find best defender (highest strength + speed)
            var bestDefender = _defenders
                .OrderByDescending(p => p.Strength + p.Speed)
                .FirstOrDefault();

            if (bestDefender != null)
            {
                var defenderPressure = (bestDefender.Strength + bestDefender.Speed) / 2.0; // 0-100
                var pressureFactor = 0.5 + (defenderPressure / 200.0); // 0.5 to 1.0 multiplier
                fumbleProbability *= pressureFactor;
            }

            // Number of defenders (gang tackles increase fumbles)
            if (_defenders.Count >= 3)
                fumbleProbability *= 1.3; // +30% for gang tackle
            else if (_defenders.Count >= 2)
                fumbleProbability *= 1.15; // +15% for 2 defenders

            // Clamp to reasonable range
            fumbleProbability = Math.Max(0.003, Math.Min(0.25, fumbleProbability));

            Occurred = _rng.NextDouble() < fumbleProbability;
        }
    }
}