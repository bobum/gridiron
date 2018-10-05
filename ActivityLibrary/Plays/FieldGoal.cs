using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //field goal or extra points
    //also a blocked field goal
    //fake field goal, which can be a run or a pass
    public sealed class FieldGoal : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            return game;
        }
    }
}
