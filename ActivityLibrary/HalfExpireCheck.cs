using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class HalfExpireCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            return game;
        }
    }
}
