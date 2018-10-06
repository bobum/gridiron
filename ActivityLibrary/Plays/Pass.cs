﻿using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //Pass plays can be your typical downfield pass play
    //a lateral
    //a halfback pass
    //a fake punt would be in the Punt class - those could be run or pass...
    public sealed class Pass : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            game.CurrentPlay.ElapsedTime += 6.5;

            return game;
        }
    }
}
