using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class FumbleOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private ICryptoRandom _rng;

        public FumbleOccurredSkillsCheck(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //was there a fumble? Totally random for now...
            var fumble = _rng.Next(2);

            Occurred = fumble == 1;
        }
    }
}