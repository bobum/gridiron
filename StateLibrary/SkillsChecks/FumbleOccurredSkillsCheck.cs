using DomainObjects;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class FumbleOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        public override void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();

            //was there a fumble? Totally random for now...
            var fumble = rng.Next(2);

            Occurred = fumble == 1;
        }
    }
}