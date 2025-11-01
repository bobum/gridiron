using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //field goal or extra points
    //also a blocked field goal
    //fake field goal, which can be a run or a pass
    //a muffed snap
    public sealed class FieldGoal : IGameAction
    {
        //need to determine if this is an extra point or a field goal attempt
        //to assign time correctly...
        public void Execute(Game game, ILogger logger)
        {
            game.CurrentPlay.ElapsedTime += 1.5;
            logger.LogInformation("The kick is up...");
        }
    }
}
