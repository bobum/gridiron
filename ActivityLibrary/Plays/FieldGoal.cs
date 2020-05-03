using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //field goal or extra points
    //also a blocked field goal
    //fake field goal, which can be a run or a pass
    //a muffed snap
    public sealed class FieldGoal : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            //need to determine if this is an extra point or a field goal attempt
            //to assign time correctly...
            game.CurrentPlay.ElapsedTime += 1.5;
            game.CurrentPlay.Result.Add("The kick is up...");

            return game;
        }
    }
}
