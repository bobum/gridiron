using DomainObjects;
using System.Activities;

namespace ActivityLibrary
{
    //fumbled snap?
    public sealed class Snap : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        
        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            CryptoRandom rng = new CryptoRandom();
            var didItHappen = rng.NextDouble();

            game.CurrentPlay.GoodSnap = didItHappen <= .01 ? false : true;

            return game;
        }
    }
}
