using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class FieldGoalResult : IGameAction
    {
        public void Execute(Game game, ILogger logger)
        {
            logger.LogInformation("Nice try kicker!");
        }
    }
}