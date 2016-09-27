using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class PreGame : CodeActivity<Game>
    {
        // Define an activity input argument of type string
        public InArgument<Team> HomeTeam { get; set; }
        public InArgument<Team> AwayTeam { get; set; }


        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override Game Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            var game = new Game
            {
                AwayTeam = AwayTeam.Get(context),
                HomeTeam = HomeTeam.Get(context)
            };
            return game;
        }
    }
}
