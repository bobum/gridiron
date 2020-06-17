using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class FieldGoalBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ICryptoRandom _rng;

        public FieldGoalBlockOccurredSkillsCheck(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //was there a fumble? Totally random for now...
            var block = _rng.Next(2);

            Occurred = block == 1;
        }
    }
}