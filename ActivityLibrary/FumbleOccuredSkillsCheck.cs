﻿using DomainObjects;
using System.Activities;

namespace ActivityLibrary
{
    //this class will look a the two teams and determine if a fumble
    //occured on any given play based on the skills of the player
    //and the type oc current play - evntaully
    //for now we'll do it randomly...
    public sealed class FumbleOccuredSkillsCheck : CodeActivity<bool>
    {
        public InArgument<Game> Game { get; set; }

        protected override bool Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            CryptoRandom rng = new CryptoRandom();

            //was there a fumble? Totally random for now...
            var fumble = rng.Next(2);
            if (fumble == 1)
            {
                
            }

            return fumble == 1;
        }
    }
}