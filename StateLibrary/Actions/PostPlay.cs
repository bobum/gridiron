using DomainObjects;
using StateLibrary.Actions.EventChecks;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class PostPlay : IGameAction
    {
        public void Execute(Game game)
        {
            //inside here we will do things like check for injuries, advance the down, change possession
            //determine if it's a hurry up offense or if they are trying to
            //kill the clock and add time appropriately...
            //add the current play to the plays list

            var penaltyCheck = new PenaltyCheck(PenaltyOccured.During);
            penaltyCheck.Execute(game);

            penaltyCheck = new PenaltyCheck(PenaltyOccured.After);
            penaltyCheck.Execute(game);

            var scoreCheck = new ScoreCheck();
            scoreCheck.Execute(game);

            var quarterExpireCheck = new QuarterExpireCheck();
            quarterExpireCheck.Execute(game);

            game.Plays.Add(game.CurrentPlay);
        }
    }
}
