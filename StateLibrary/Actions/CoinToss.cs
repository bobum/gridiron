using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class CoinToss : IGameAction
    {
        public void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            game.Possession = toss == 1 ? Possession.Away : Possession.Home;
        }
    }
}
