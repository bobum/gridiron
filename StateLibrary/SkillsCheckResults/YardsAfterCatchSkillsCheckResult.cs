using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.BaseClasses;
using StateLibrary.SkillsChecks;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates yards after catch based on receiver's ability to break tackles and elude defenders.
    /// Integrates with YardsAfterCatchSkillsCheck to determine if receiver has opportunity for YAC.
    /// </summary>
    public class YardsAfterCatchSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private Player _receiver;

        public YardsAfterCatchSkillsCheckResult(ISeedableRandom rng, Player receiver)
        {
            _rng = rng;
            _receiver = receiver;
        }

        public override void Execute(Game game)
        {
            // Check for YAC opportunity
            var yacCheck = new YardsAfterCatchSkillsCheck(_rng, _receiver);
            yacCheck.Execute(game);

            if (!yacCheck.Occurred)
            {
                // Tackled immediately (0-2 yards)
                Result = _rng.Next(0, 3);
                return;
            }

            // Good YAC opportunity - receiver breaks tackles
            var yacPotential = (_receiver.Speed + _receiver.Agility + _receiver.Rushing) / 3.0;
            var baseYAC = 3.0 + (yacPotential / 20.0); // 3-8 yards typically

            // Add randomness (-2 to +6 yards)
            var randomFactor = (_rng.NextDouble() * 8) - 2;
            var totalYAC = Math.Max(0, (int)Math.Round(baseYAC + randomFactor));

            // 5% chance for big play after catch if receiver is fast
            if (_rng.NextDouble() < 0.05 && _receiver.Speed > 85)
            {
                totalYAC += _rng.Next(10, 30);
                game.CurrentPlay.Result.LogInformation($"{_receiver.LastName} breaks free! Great run after catch!");
            }

            Result = totalYAC;
        }
    }
}
