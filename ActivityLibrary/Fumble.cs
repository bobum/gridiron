using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{
    //we have a fumble on the play
    //and we should now know who has the ball
    //determine the result of the fumble and add it to the 
    //current play
    public sealed class Fumble : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }
        public InArgument<Possession> CurrentPossession { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);
            var possession = CurrentPossession.Get(context);

            //first determine if there was a possession change on the play
            game.CurrentPlay.PossessionChange = possession != game.Possession;

            //set the correct possession in the game
            game.Possession = possession;

            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.Add("Fumble on the play");
            if (game.CurrentPlay.PossessionChange)
            {
                game.CurrentPlay.Result.Add("Possession changes hands");
                game.CurrentPlay.Result.Add($"{game.Possession} now has possession");
            }
            else
            {
                game.CurrentPlay.Result.Add($"{game.Possession} keeps possession");
            }

            return game;
        }
    }
}
