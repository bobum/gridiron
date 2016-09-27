﻿using System;
using System.Linq;
using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{
    public sealed class PenaltyCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        
        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            //later we will wire in a real penalty check and allow for offsetting penalties,
            //all penalties that are appropriate for the type of play,
            //penalties that stop the play (like false start)
            //declined penalties etc, multiple penalties etc...
            //for now - if we already have a penalty, skip it...
            if (game.CurrentPlay.Penalty == null ||
                game.CurrentPlay.Penalty.Name == PenaltyNames.NoPenalty)
            {
                CryptoRandom rng = new CryptoRandom();
                var didItHappen = rng.NextDouble();
                var homeAway = rng.NextDouble();
                Console.WriteLine(homeAway);
                var havePenalty = false;

                switch (game.CurrentPlay.PlayType)
                {
                    case PlayType.Kickoff:
                        if (didItHappen <=
                            Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Odds)
                        {
                            game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.IllegalBlockAbovetheWaist);
                            havePenalty = true;
                        }
                        break;
                    case PlayType.FieldGoal:
                        if (didItHappen <=
                            Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.RoughingtheKicker).Odds)
                        {
                            game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.RoughingtheKicker);
                            havePenalty = true;
                        }
                        break;
                    case PlayType.Pass:
                        if (didItHappen <=
                            Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.OffensiveHolding).Odds)
                        {
                            game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.OffensiveHolding);
                            havePenalty = true;
                        }
                        break;
                    case PlayType.Punt:
                        if (didItHappen <=
                            Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.RoughingtheKicker).Odds)
                        {
                            game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.RoughingtheKicker);
                            havePenalty = true;
                        }
                        break;
                    case PlayType.Run:
                        if (didItHappen <=
                            Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.OffensiveHolding).Odds)
                        {
                            game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                                    p.Name == PenaltyNames.OffensiveHolding);
                            havePenalty = true;
                        }
                        break;
                }

                if (!havePenalty)
                {
                    game.CurrentPlay.Penalty = Penalties.List.Single(p =>
                            p.Name == PenaltyNames.NoPenalty);
                }
                else
                {
                    game.CurrentPlay.Penalty.CalledOn = 
                        homeAway <= game.CurrentPlay.Penalty.AwayOdds ? 
                        Posession.Away : Posession.Home;
                    Console.WriteLine(game.CurrentPlay.Penalty.CalledOn);
                }
            }

            return game;
        }
    }
}