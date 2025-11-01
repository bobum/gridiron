using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    //we have a fumble on the play
    //and we should now know who has the ball
    //determine the result of the fumble and add it to the 
    //current play
    public sealed class Fumble : IGameAction
    {
        private readonly Possession _possession;

        public Fumble(Possession possession)
        {
            _possession = possession;
        }

        public void Execute(Game game)
        {
            //first determine if there was a possession change on the play
            game.CurrentPlay.PossessionChange = _possession != game.CurrentPlay.Possession;

            //set the correct possession in the game
            game.CurrentPlay.Possession = _possession;

            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.LogInformation("Fumble on the play");
            if (game.CurrentPlay.PossessionChange)
            {
                game.CurrentPlay.Result.LogInformation("Possession changes hands");
                game.CurrentPlay.Result.LogInformation($"{game.CurrentPlay.Possession} now has possession");
            }
            else
            {
                game.CurrentPlay.Result.LogInformation($"{game.CurrentPlay.Possession} keeps possession");
            }

            //now we know somebody bobbled the ball, and somebody recovered it - add that in the play for the records
            //we'll fill in the players involved in the fumble later
            game.CurrentPlay.Fumbles.Add(new DomainObjects.Fumble());
        }
    }
}
