using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //Punt could be a regular punt,
    //or a fake punt pass
    //or a fake punt run
    //blocked punt
    //a muffed snap
    public sealed class Punt : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.Add("Long punt");
        }
    }
}
