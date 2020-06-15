using DomainObjects;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class FieldGoalBlockOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        public override void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();

            //was there a fumble? Totally random for now...
            var block = rng.Next(2);

            Occurred = block == 1;
        }
    }
}