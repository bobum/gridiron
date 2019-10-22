using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class GameExpireCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            //in here is where we need to check to see if the game is over and not tied...
            //if it is tied this is where we would need to set up overtime quarters 
            //and extend the game etc
            var game = Game.Get(context);

            return game;
        }
    }
}
