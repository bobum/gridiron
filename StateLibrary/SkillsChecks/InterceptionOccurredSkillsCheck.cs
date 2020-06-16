using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class InterceptionOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ICryptoRandom _rng;
        public InterceptionOccurredSkillsCheck(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //was there an interception? Totally random for now...
            var interception = _rng.Next(2);

            Occurred = interception == 1;
        }
    }
}