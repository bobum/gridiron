using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;
using StateLibrary.Interfaces;

namespace StateLibrary.SkillsCheckResults
{
    public class FumblePossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        private ICryptoRandom _rng;
        public FumblePossessionChangeSkillsCheckResult(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //there was a fumble - who got it?
            var toss = _rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}