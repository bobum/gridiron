using DomainObjects;
using Microsoft.Extensions.Logging;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class Snap : IGameAction
    {
        private ISeedableRandom _rng;
        public Snap(ISeedableRandom rng)
        {
            _rng = rng;
        }

        public void Execute(Game game, ILogger logger)
        {
            var didItHappen = _rng.NextDouble();

            //we can't have a muffed snap on a kick off - so don't even check
            game.CurrentPlay.GoodSnap = true;

            if (game.CurrentPlay.PlayType != PlayType.Kickoff)
            {
                game.CurrentPlay.GoodSnap = !(didItHappen <= .01);
            }

            game.CurrentPlay.ElapsedTime += game.CurrentPlay.GoodSnap ? 0.2 : 0.5;

            logger.LogInformation(game.CurrentPlay.GoodSnap
                ? "Good snap..."
                : "Oh no!  The snap is muffed - players are scrambling for the ball...");

            //TODO: Handle a muffed snap in every playtype except a kickoff
        }
    }
}
