using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Determines fumble recovery and calculates return yardage
    /// </summary>
    public class FumbleRecoverySkillsCheckResult : SkillsCheckResult<FumbleRecoveryResult>
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _fumbler;
        private readonly List<Player> _offensePlayers;
        private readonly List<Player> _defensePlayers;
        private readonly int _fumbleSpot;

        public FumbleRecoverySkillsCheckResult(
            ISeedableRandom rng,
            Player fumbler,
            List<Player> offensePlayers,
            List<Player> defensePlayers,
            int fumbleSpot)
        {
            _rng = rng;
            _fumbler = fumbler;
            _offensePlayers = offensePlayers;
            _defensePlayers = defensePlayers;
            _fumbleSpot = fumbleSpot;
        }

        public override void Execute(Game game)
        {
            // Check if fumble goes out of bounds
            var outOfBoundsChance = GameProbabilities.Turnovers.FUMBLE_OUT_OF_BOUNDS_PROBABILITY;
            var goesOutOfBounds = _rng.NextDouble() < outOfBoundsChance;

            if (goesOutOfBounds)
            {
                Result = new FumbleRecoveryResult
                {
                    OutOfBounds = true,
                    RecoveredBy = null, // Original team keeps possession (handled by caller)
                    RecoverySpot = _fumbleSpot,
                    ReturnYards = 0
                };
                return;
            }

            // Determine bounce direction
            var bounceRandom = _rng.NextDouble();
            int bounceYards;
            double offenseRecoveryChance;

            if (bounceRandom < GameProbabilities.Turnovers.FUMBLE_RECOVERY_BACKWARD_THRESHOLD)
            {
                // Backward bounce (toward offense's goal)
                bounceYards = -(int)(_rng.NextDouble() * 8); // -8 to 0 yards
                offenseRecoveryChance = GameProbabilities.Turnovers.FUMBLE_RECOVERY_BACKWARD_BASE;
            }
            else if (bounceRandom < GameProbabilities.Turnovers.FUMBLE_RECOVERY_FORWARD_THRESHOLD)
            {
                // Forward bounce (toward defense's goal)
                bounceYards = (int)(_rng.NextDouble() * 8); // 0 to 8 yards
                offenseRecoveryChance = GameProbabilities.Turnovers.FUMBLE_RECOVERY_FORWARD_BASE;
            }
            else
            {
                // Sideways/minimal bounce
                bounceYards = (int)((_rng.NextDouble() * 4) - 2); // -2 to 2 yards
                offenseRecoveryChance = GameProbabilities.Turnovers.FUMBLE_RECOVERY_SIDEWAYS_BASE;
            }

            // Adjust recovery chance based on player awareness
            var offenseAvgAwareness = _offensePlayers.Average(p => p.Awareness);
            var defenseAvgAwareness = _defensePlayers.Average(p => p.Awareness);
            var awarenessDiff = (offenseAvgAwareness - defenseAvgAwareness) / 100.0;
            offenseRecoveryChance += awarenessDiff * GameProbabilities.Turnovers.FUMBLE_RECOVERY_AWARENESS_FACTOR;

            // Clamp to reasonable bounds
            offenseRecoveryChance = Math.Max(
                GameProbabilities.Turnovers.FUMBLE_RECOVERY_MIN_CLAMP,
                Math.Min(GameProbabilities.Turnovers.FUMBLE_RECOVERY_MAX_CLAMP, offenseRecoveryChance));

            // Determine who recovers
            var offenseRecovers = _rng.NextDouble() < offenseRecoveryChance;

            Player recoverer;
            int returnYards;

            if (offenseRecovers)
            {
                // Offense recovers - usually near fumble spot
                recoverer = _offensePlayers
                    .OrderByDescending(p => p.Speed + p.Awareness)
                    .FirstOrDefault() ?? _fumbler;

                // Minimal return (usually just fall on it)
                returnYards = bounceYards + (int)((_rng.NextDouble() * 6) - 3); // bounce +/- 3 yards
            }
            else
            {
                // Defense recovers - can advance
                recoverer = _defensePlayers
                    .OrderByDescending(p => p.Speed + p.Awareness)
                    .FirstOrDefault();

                if (recoverer == null)
                {
                    // Fallback
                    Result = new FumbleRecoveryResult
                    {
                        OutOfBounds = false,
                        RecoveredBy = _fumbler,
                        RecoverySpot = _fumbleSpot + bounceYards,
                        ReturnYards = bounceYards
                    };
                    return;
                }

                // Calculate return yardage (scoop and score potential)
                var returnerSkill = (recoverer.Speed + recoverer.Agility) / 2.0;
                var baseReturn = 5.0 + (returnerSkill / 100.0) * 15.0; // 5-20 yards base
                var randomFactor = (_rng.NextDouble() * 80.0) - 30.0; // -30 to +50 variance

                returnYards = bounceYards + (int)(baseReturn + randomFactor);
            }

            var recoverySpot = _fumbleSpot + bounceYards;

            Result = new FumbleRecoveryResult
            {
                OutOfBounds = false,
                RecoveredBy = recoverer,
                RecoverySpot = recoverySpot,
                ReturnYards = returnYards
            };
        }
    }

    /// <summary>
    /// Result of fumble recovery calculation
    /// </summary>
    public class FumbleRecoveryResult
    {
        public bool OutOfBounds { get; set; }
        public Player? RecoveredBy { get; set; }
        public int RecoverySpot { get; set; }
        public int ReturnYards { get; set; }
    }
}
