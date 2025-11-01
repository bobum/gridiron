using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class FieldGoalResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.LogInformation("Nice try kicker!");
        }
    }
}