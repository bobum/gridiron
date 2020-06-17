using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class RunResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.Add("Runner is down");
        }
    }
}