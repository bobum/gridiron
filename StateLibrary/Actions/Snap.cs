using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class Snap : IGameAction
    {
        public void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();
            var didItHappen = rng.NextDouble();

            //we can't have a muffed snap on a kick off - so don't even check
            game.CurrentPlay.GoodSnap = true;

            if (game.CurrentPlay.PlayType != PlayType.Kickoff)
            {
                game.CurrentPlay.GoodSnap = !(didItHappen <= .01);
            }

            game.CurrentPlay.ElapsedTime += game.CurrentPlay.GoodSnap ? 0.2 : 0.5;

            game.CurrentPlay.Result.Add(game.CurrentPlay.GoodSnap
                ? "Good snap..."
                : "Oh no!  The snap is muffed - players are scrambling for the ball...");

            //TODO: Handle a muffed snap in every playtype except a kickoff
        }
    }
}
