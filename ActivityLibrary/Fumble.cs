using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class Fumble : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            //for now this is a totally random determination of who gets
            //the ball after a fumble and we change the possestion of the game
            CryptoRandom rng = new CryptoRandom();
            var game = Game.Get(context);
            var toss = rng.Next(2);
            var preFumble = game.Posession;
            game.Posession = toss == 1 ? Posession.Away : Posession.Home;
            game.CurrentPlay.PossessionChange = preFumble != game.Posession;
            return game;
        }
    }
}
