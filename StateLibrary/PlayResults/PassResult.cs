using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class PassResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.Add("Pass play is complete...");
        }
    }
}