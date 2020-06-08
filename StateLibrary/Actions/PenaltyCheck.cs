using System.Linq;
using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    class PenaltyCheck : IGameAction
    {
        private readonly PenaltyOccured _penaltyOccured;
        public PenaltyCheck(PenaltyOccured penaltyOccured)
        {
            _penaltyOccured = penaltyOccured;
        }

        public void Execute(Game game)
        {
            CryptoRandom rng = new CryptoRandom();
            var didItHappen = rng.NextDouble();
            var homeAway = rng.NextDouble();
            var havePenalty = false;
            var currentPenalty = new Penalty() { Occured = _penaltyOccured };

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
                    Possession.Away : Possession.Home;
                game.CurrentPlay.Penalties.Add(currentPenalty);
                game.CurrentPlay.Result.Add("Flag on the play");
            }
        }
    }
}
