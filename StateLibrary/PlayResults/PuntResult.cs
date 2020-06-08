using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class PuntResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.Add("The punt falls...");
        }
    }
}