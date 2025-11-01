using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions.EventChecks
{
    public sealed class GameExpireCheck : IGameAction
    {
        public void Execute(Game game, ILogger logger)
        {
            //in here is where we need to check to see if the game is over and not tied...
            //if it is tied this is where we would need to set up overtime quarters 
            //and extend the game etc
        }
    }
}
