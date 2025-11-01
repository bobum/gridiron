using DomainObjects;
using Microsoft.Extensions.Logging;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public sealed class Interception : IGameAction
    {
        private readonly Possession _possession;

        public Interception(Possession possession)
        {
            _possession = possession;
        }

        public void Execute(Game game, ILogger logger)
        {
            //there was a possession change on the play
            game.CurrentPlay.PossessionChange = true;

            //set the correct possession in the game
            game.CurrentPlay.Possession = _possession;
            game.CurrentPlay.ElapsedTime += 0.5;
            logger.LogInformation("Interception!!");
            logger.LogInformation("Possession changes hands");
            logger.LogInformation($"{game.CurrentPlay.Possession} now has possession");

            //now we know somebody bobbled the ball, and somebody recovered it - add that in the play for the records
            game.CurrentPlay.Interception = true;
        }
    }
}
