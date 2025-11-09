using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates yards lost when a QB is sacked.
    /// Returns a negative value representing the yardage loss.
    /// </summary>
    public class SackYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private int _fieldPosition;

        public SackYardsSkillsCheckResult(ISeedableRandom rng, int fieldPosition)
        {
            _rng = rng;
            _fieldPosition = fieldPosition;
        }

        public override void Execute(Game game)
        {
            // Calculate sack yardage loss (2-10 yards typically)
            var sackYards = -1 * _rng.Next(2, 11);

            // Don't go past own goal line (can't lose more yards than field position)
            var maxLoss = -1 * _fieldPosition;
            Result = Math.Max(sackYards, maxLoss);
        }
    }
}
