using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class KickoffResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.LogInformation("Kickoff squad leaves the field...");
        }
    }
}