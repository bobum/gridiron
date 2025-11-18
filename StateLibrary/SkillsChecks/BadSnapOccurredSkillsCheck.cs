using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Configuration;

namespace StateLibrary.SkillsChecks
{
    /// <summary>
    /// Checks if a bad snap occurs on a punt.
    /// Bad snaps are rare but can be catastrophic.
    /// </summary>
    public class BadSnapOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly ISeedableRandom _rng;
        private readonly Player _longSnapper;

        public BadSnapOccurredSkillsCheck(ISeedableRandom rng, Player longSnapper)
        {
            _rng = rng;
            _longSnapper = longSnapper;
        }

        public override void Execute(Game game)
        {
            // Bad snap probability based on long snapper's skill
            // Average LS (50 skill): ~2% chance
            // Good LS (70+ skill): ~0.5% chance
            // Poor LS (30 skill): ~5% chance

            var skillFactor = _longSnapper.Blocking / GameProbabilities.Punts.PUNT_BAD_SNAP_SKILL_DENOMINATOR;
            var badSnapChance = GameProbabilities.Punts.PUNT_BAD_SNAP_BASE
                - (skillFactor * GameProbabilities.Punts.PUNT_BAD_SNAP_SKILL_FACTOR);

            Occurred = _rng.NextDouble() < badSnapChance;
        }
    }
}
