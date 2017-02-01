using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class PostPlay : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            //inside here we will do things like check for injuries
            var game = Game.Get(context);

            return game;
        }
    }
}