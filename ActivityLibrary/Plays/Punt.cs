using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //Punt could be a regular punt,
    //or a fake punt pass
    //or a fake punt run
    //blocked punt
    public sealed class Punt : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.Add("Long punt");

            return game;
        }
    }
}
