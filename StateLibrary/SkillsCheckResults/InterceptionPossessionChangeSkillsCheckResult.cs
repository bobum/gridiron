using DomainObjects;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    public class InterceptionPossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        public override void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}