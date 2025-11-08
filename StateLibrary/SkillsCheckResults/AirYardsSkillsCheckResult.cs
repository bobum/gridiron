using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    /// <summary>
    /// Calculates air yards for a pass based on pass type and field position.
    /// Air yards represent the distance the ball travels in the air before being caught.
    /// </summary>
    public class AirYardsSkillsCheckResult : YardageSkillsCheckResult
    {
        private ISeedableRandom _rng;
        private PassType _passType;
        private int _fieldPosition;

        public AirYardsSkillsCheckResult(ISeedableRandom rng, PassType passType, int fieldPosition)
        {
            _rng = rng;
            _passType = passType;
            _fieldPosition = fieldPosition;
        }

        public override void Execute(Game game)
        {
            var yardsToGoal = 100 - _fieldPosition;

            Result = _passType switch
            {
                PassType.Screen => _rng.Next(-3, 3),     // -3 to 2 yards (behind LOS to short)
                PassType.Short => _rng.Next(3, Math.Max(4, Math.Min(12, yardsToGoal))),   // 3-11 yards
                PassType.Forward => _rng.Next(8, Math.Max(9, Math.Min(20, yardsToGoal))), // 8-19 yards
                PassType.Deep => _rng.Next(18, Math.Max(19, Math.Min(45, yardsToGoal))),  // 18-44 yards
                _ => _rng.Next(5, Math.Max(6, Math.Min(15, yardsToGoal)))                 // Default 5-14 yards
            };
        }
    }
}
