using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //Pass plays can be your typical downfield pass play
    //a lateral
    //a halfback pass
    //a fake punt would be in the Punt class - those could be run or pass...
    //a muffed snap
    public sealed class Pass : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.Add("Pass downfield!");
        }
    }
}
