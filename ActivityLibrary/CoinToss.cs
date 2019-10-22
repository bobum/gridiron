using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class CoinToss : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            CryptoRandom rng = new CryptoRandom();
            var game = Game.Get(context);
            var toss = rng.Next(2);
            game.Possession = toss == 1 ? Possession.Away : Possession.Home;
            return game;
        }
    }
}
