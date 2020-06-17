using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class KickoffResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.Add("Kickoff squad leaves the field...");
        }
    }
}