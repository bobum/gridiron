using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class PassResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.LogInformation("Pass play is complete...");
        }
    }
}