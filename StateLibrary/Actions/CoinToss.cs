using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class CoinToss : IGameAction
    {
        private ICryptoRandom _rng;
        public CoinToss(ICryptoRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game)
        {
            var toss = _rng.Next(2);
            game.Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}
