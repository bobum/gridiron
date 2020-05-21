using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{

    public sealed class Interception : IGameAction
    {
        public void Execute(Game game)
        {
            //for now this is a totally random determination of who gets
            //the ball after an interception and we change the possession of the game
            CryptoRandom rng = new CryptoRandom();
            var toss = rng.Next(2);
            var preInterception = game.Possession;
            game.Possession = toss == 1 ? Possession.Away : Possession.Home;
            game.CurrentPlay.PossessionChange = preInterception != game.Possession;
        }
    }
}
