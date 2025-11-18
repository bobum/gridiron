using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
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
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_QB_SACK_PROBABILITY;
            }
            else if (_playType == PlayType.Kickoff || _playType == PlayType.Punt)
            {
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_RETURN_PROBABILITY;
            }
            else
            {
                fumbleProbability = GameProbabilities.Turnovers.FUMBLE_NORMAL_PROBABILITY;
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
                fumbleProbability *= GameProbabilities.Turnovers.FUMBLE_GANG_TACKLE_MULTIPLIER;
            else if (_defenders.Count >= 2)
                fumbleProbability *= GameProbabilities.Turnovers.FUMBLE_TWO_DEFENDERS_MULTIPLIER;

            // Clamp to reasonable range
            fumbleProbability = Math.Max(
                GameProbabilities.Turnovers.FUMBLE_MIN_CLAMP,
                Math.Min(GameProbabilities.Turnovers.FUMBLE_MAX_CLAMP, fumbleProbability));

            Occurred = _rng.NextDouble() < fumbleProbability;
        }
    }
}