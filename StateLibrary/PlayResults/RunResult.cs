using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class RunResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.LogInformation("Runner is down");
        }
    }
}