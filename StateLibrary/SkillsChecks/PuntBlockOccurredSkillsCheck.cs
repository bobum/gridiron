using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class PuntBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ISeedableRandom _rng;
        public PuntBlockOccurredSkillsCheck(ISeedableRandom rng)
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