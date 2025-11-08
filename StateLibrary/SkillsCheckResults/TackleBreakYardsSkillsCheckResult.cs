using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates extra yards gained when a ball carrier breaks a tackle.
    /// Typically adds 3-8 yards to the run.
    /// </summary>
    public class TackleBreakYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private readonly ISeedableRandom _rng;

        public TackleBreakYardsSkillsCheckResult(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            // Tackle break adds 3-8 yards
            Result = _rng.Next(3, 9);
        }
    }
}
