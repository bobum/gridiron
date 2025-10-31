using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class FieldGoalBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;

        public FieldGoalBlockOccurredSkillsCheck(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //was the fieldgoal blocked? Totally random for now...
            var block = _rng.Next(2);

            Occurred = block == 1;
        }
    }
}