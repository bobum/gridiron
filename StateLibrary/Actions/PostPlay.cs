using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Actions.EventChecks;
using StateLibrary.BaseClasses;

namespace StateLibrary.Actions
{
    public class PostPlay : PlayBookend
    {
        public PostPlay(ICryptoRandom rng) : base(rng)
        {
        }

        public override void Execute(Game game)
        {
            //inside here we will do things like check for injuries, advance the down, change possession
            //determine if it's a hurry up offense or if they are trying to
            //kill the clock and add time appropriately...
            //add the current play to the plays list

            //if we have a pre-snap penalty - no need to check for others
            if (game.CurrentPlay.Penalties.Count == 0)
            {
                PenaltyCheck(PenaltyOccuredWhen.During, game);
                PenaltyCheck(PenaltyOccuredWhen.After, game);
            }

            var scoreCheck = new ScoreCheck();
            scoreCheck.Execute(game);

            var injuryCheck = new InjuryCheck();
            injuryCheck.Execute(game);

            var advanceDown = new AdvanceDown();
            advanceDown.Execute(game);

            var penaltyResult = new PenaltyApplication();
            penaltyResult.Execute(game);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            var halftimeCheck = new HalfExpireCheck();
            halftimeCheck.Execute(game);

            game.Plays.Add(game.CurrentPlay);
        }
    }
}