using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class CoinToss : CodeActivity<Game>
    {
        // Define an activity input argument of type string
        public InArgument<Game> Game { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override Game Execute(CodeActivityContext context)
        {
            CryptoRandom rng = new CryptoRandom();
            var game = Game.Get(context);
            var toss = rng.Next(2);
            game.Posession = toss == 1 ? Posession.Away : Posession.Home;
            return game;
        }
    }
}
