using System;
using System.Linq;
using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{
    public sealed class PenaltyCheck : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        public InArgument<PenaltyOccured> Occured { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);
            var occurred = Occured.Get(context);

            //later we will wire in a real penalty check and allow for offsetting penalties,
            //all penalties that are appropriate for the type of play,
            //penalties that stop the play (like false start)
            //declined penalties etc, multiple penalties etc...

            CryptoRandom rng = new CryptoRandom();
            var didItHappen = rng.NextDouble();
            var homeAway = rng.NextDouble();
            var havePenalty = false;
            var currentPenalty = new DomainObjects.Penalty() { Occured = occurred };

            switch (game.CurrentPlay.PlayType)
            {
                case PlayType.Kickoff:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Odds)
                    {
                        currentPenalty = Penalties.List.Single(p =>
                                p.Name == PenaltyNames.IllegalBlockAbovetheWaist);
                        havePenalty = true;
                    }
                    break;
                case PlayType.FieldGoal:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker).Odds)
                    {
                        currentPenalty = Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker);
                        havePenalty = true;
                    }
                    break;
                case PlayType.Pass:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding).Odds)
                    {
                        currentPenalty = Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding);
                        havePenalty = true;
                    }
                    break;
                case PlayType.Punt:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker).Odds)
                    {
                        currentPenalty = Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker);
                        havePenalty = true;
                    }
                    break;
                case PlayType.Run:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding).Odds)
                    {
                        currentPenalty = Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding);
                        havePenalty = true;
                    }
                    break;
            }

            if (havePenalty)
            {
                currentPenalty.CalledOn =
                    homeAway <= currentPenalty.AwayOdds ?
                    Posession.Away : Posession.Home;
                game.CurrentPlay.Penalties.Add(currentPenalty);
            }

            return game;
        }
    }
}