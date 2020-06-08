using DomainObjects;
using StateLibrary.BaseClasses;
using StateLibrary.Interfaces;

namespace StateLibrary.SkillsCheckResults
{
    public class FumblePossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        public override void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}