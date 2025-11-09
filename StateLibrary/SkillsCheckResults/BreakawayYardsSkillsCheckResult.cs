using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates extra yards gained when a ball carrier breaks into the open field.
    /// Typically adds 15-44 yards on big run breakaways.
    /// </summary>
    public class BreakawayYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;

        public BreakawayYardsSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            // Breakaway run adds 15-44 yards
            Result = _rng.Next(15, 45);
        }
    }
}
