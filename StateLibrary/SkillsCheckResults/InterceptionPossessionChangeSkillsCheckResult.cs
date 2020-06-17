using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    public class InterceptionPossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        public override void Execute(Game game)
        {
            //we know that an interception has occurred - so we change possession
            Possession = game.CurrentPlay.Possession == Possession.Away ? Possession.Home : Possession.Away;
        }
    }
}