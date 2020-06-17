using DomainObjects;
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
        public void Execute(Game game)
        {
            game.CurrentPlay.ElapsedTime += 1.5;
            game.CurrentPlay.Result.Add("The kick is up...");
        }
    }
}
