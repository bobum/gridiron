using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //Run plays can be your typical, hand it off to the guy play
    //or a QB scramble
    //or a 2-pt conversion
    //or a kneel
    //a fake punt would be in the Punt class - those could be run or pass...
    public sealed class Run : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            return game;
        }
    }
}
