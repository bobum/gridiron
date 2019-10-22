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
            //deterine if it's a hurry up offense or if they are trying to
            //kill the clock and add time appropriately
            var game = Game.Get(context);

            //add the current play to the plays list
            game.Plays.Add(game.CurrentPlay);

            return game;
        }
    }
}