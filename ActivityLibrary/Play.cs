using DomainObjects;
using System.Activities;

namespace ActivityLibrary
{

    public sealed class Play : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        
        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            return game;
        }
    }
}
