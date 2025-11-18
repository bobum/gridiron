using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

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

            // Base chance for good YAC opportunity, increased by receiver skills above threshold
            var yacBonus = (yacPotential - GameProbabilities.Passing.YAC_SKILL_THRESHOLD)
                / GameProbabilities.Passing.YAC_SKILL_DENOMINATOR;
            var yacProbability = GameProbabilities.Passing.YAC_OPPORTUNITY_BASE_PROBABILITY + yacBonus;

            // Clamp to reasonable bounds
            yacProbability = Math.Max(
                GameProbabilities.Passing.YAC_MIN_CLAMP,
                Math.Min(GameProbabilities.Passing.YAC_MAX_CLAMP, yacProbability));

            Occurred = _rng.NextDouble() < yacProbability;
        }
    }
}
