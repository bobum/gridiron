using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class ScoreCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        
        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            return game;
        }
    }
}
