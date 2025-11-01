using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //Pass plays can be your typical downfield pass play
    //a lateral
    //a halfback pass
    //a fake punt would be in the Punt class - those could be run or pass...
    //a fake fieldgoal would be in the FieldGoal class - those could be run or pass...
    //a muffed snap on a punt would be in the Punt class - those could be run or pass...
    //a muffed snap on a fieldgoald would be in the FieldGoal class - those could be run or pass...
    public sealed class Pass : IGameAction
    {
        public void Execute(Game game, ILogger logger)
        {
            game.CurrentPlay.ElapsedTime += 6.5;
            logger.LogInformation("Pass downfield!");
        }
    }
}
