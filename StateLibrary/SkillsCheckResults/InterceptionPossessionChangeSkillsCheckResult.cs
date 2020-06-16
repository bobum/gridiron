using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsCheckResults
{
    public class InterceptionPossessionChangeSkillsCheckResult : PossessionChangeSkillsCheckResult
    {
        private ICryptoRandom _rng;
        public InterceptionPossessionChangeSkillsCheckResult(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            var toss = _rng.Next(2);
            Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}