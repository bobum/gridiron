using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //Run plays can be your typical, hand it off to the guy play
    //or a QB scramble
    //or a 2-pt conversion
    //or a kneel
    //a fake punt would be in the Punt class - those could be run or pass...
    //a muffed snap
    public sealed class Run : IGameAction
    {
        public void Execute(Game game)
        {
            game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.LogInformation("Run play executed");
        }
    }
}
