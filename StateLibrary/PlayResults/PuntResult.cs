using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class PuntResult : IGameAction
    {
        public void Execute(Game game, ILogger logger)
        {
            logger.LogInformation("The punt falls...");
        }
    }
}