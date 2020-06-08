using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.PlayResults
{
    public class FieldGoalResult : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.Result.Add("Nice try kicker!");
        }
    }
}