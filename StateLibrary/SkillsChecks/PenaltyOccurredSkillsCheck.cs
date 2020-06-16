﻿using System.Linq;
using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.BaseClasses;

namespace StateLibrary.SkillsChecks
{
    public class PenaltyOccurredSkillsCheck : ActionOccurredSkillsCheck
    {
        private readonly PenaltyOccuredWhen _penaltyOccuredWhen;

        public Penalty Penalty { get; private set; }

        private ICryptoRandom _rng;

        public PenaltyOccurredSkillsCheck(PenaltyOccuredWhen penaltyOccuredWhen, ICryptoRandom rng)
        {
            _penaltyOccuredWhen = penaltyOccuredWhen;
            _rng = rng;
        }

        public override void Execute(Game game)
        {
            //was there a penalty? Totally random for now...
            //in the future - we will determine, based on skills, who a penalty was called on and add that to the game object
            //there might be more than 1 penalty on a play
            Occurred = throwawayfakecodetodetermineoccurance(game);

            //let's start to populate a penalty if it occurred
            if (Occurred)
            {
                //aha - we have a penalty - let's make a new empty one...
                Penalty = new Penalty();

                //what team did the offender play for?
                var homeAway = _rng.NextDouble();
                var calledOn = homeAway <= Penalty.AwayOdds ? Possession.Away : Possession.Home;

                //who was it on that team?
                var index = _rng.Next(50);
                var player = calledOn == Possession.Away ? game.AwayTeam.Players[index] : game.HomeTeam.Players[index];

                //fill in the blanks
                Penalty.OccuredWhen = _penaltyOccuredWhen;
                Penalty.Player = player;
                Penalty.CalledOn = calledOn;
            }
        }

        private bool throwawayfakecodetodetermineoccurance(Game game)
        {
            var didItHappen = _rng.NextDouble();
            var havePenalty = false;

            switch (game.CurrentPlay.PlayType)
            {
                case PlayType.Kickoff:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.IllegalBlockAbovetheWaist).Odds)
                    {
                        havePenalty = true;
                    }
                    break;
                case PlayType.FieldGoal:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker).Odds)
                    {
                        havePenalty = true;
                    }
                    break;
                case PlayType.Pass:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding).Odds)
                    {
                        havePenalty = true;
                    }
                    break;
                case PlayType.Punt:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.RoughingtheKicker).Odds)
                    {
                        havePenalty = true;
                    }
                    break;
                case PlayType.Run:
                    if (didItHappen <=
                        Penalties.List.Single(p =>
                                p.Name == PenaltyNames.OffensiveHolding).Odds)
                    {
                        havePenalty = true;
                    }
                    break;
            }

            return havePenalty;
        }
    }
}