using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public sealed class Interception : IGameAction
    {
        private readonly Possession _possession;

        public Interception(Possession possession)
        {
            _possession = possession;
        }

        public void Execute(Game game)
        {
            //there was a possession change on the play
            game.CurrentPlay.PossessionChange = true;

            //set the correct possession in the game
            game.Possession = _possession;
            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.Add("Interception!!");
            game.CurrentPlay.Result.Add("Possession changes hands");
            game.CurrentPlay.Result.Add($"{game.Possession} now has possession");

            //now we know somebody bobbled the ball, and somebody recovered it - add that in the play for the records
            game.CurrentPlay.Interception = true;
        }
    }
}
