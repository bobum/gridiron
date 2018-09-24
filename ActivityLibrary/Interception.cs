using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class Interception : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            //for now this is a totally random determination of who gets
            //the ball after an interception and we change the possestion of the game
            CryptoRandom rng = new CryptoRandom();
            var game = Game.Get(context);
            var toss = rng.Next(2);
            var preInterception = game.Posession;
            game.Posession = toss == 1 ? Posession.Away : Posession.Home;
            game.CurrentPlay.PossessionChange = preInterception != game.Posession;
            return game;
        }
    }
}
