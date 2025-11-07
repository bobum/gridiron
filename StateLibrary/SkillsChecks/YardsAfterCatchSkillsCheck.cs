using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Determines if receiver breaks tackles for extra yards after catch
    /// </summary>
    public class YardsAfterCatchSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        private Player _receiver;

        public YardsAfterCatchSkillsCheck(ISeedableRandom rng, Player receiver)
        {
            _rng = rng;
            _receiver = receiver;
        }

        public override void Execute(Game game)
        {
            // Calculate receiver's YAC potential based on speed, agility, and elusiveness
            var yacPotential = (_receiver.Speed + _receiver.Agility + _receiver.Rushing) / 3.0;

            // Base 35% chance for good YAC opportunity, increased by receiver skills
            var yacBonus = (yacPotential - 70) / 400.0; // 0-7.5% bonus for skills above 70
            var yacProbability = 0.35 + yacBonus;

            // Clamp between 15% and 55%
            yacProbability = Math.Max(0.15, Math.Min(0.55, yacProbability));

            Occurred = _rng.NextDouble() < yacProbability;
        }
    }
}
