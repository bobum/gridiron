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
        public InArgument<Possession> CurrentPosession { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);
            var currentPosession = CurrentPosession.Get(context);

            //first determine if there was a posession change on the play
            game.CurrentPlay.PossessionChange = currentPosession != game.Possession;

            //set the correct posession in the game
            game.Possession = currentPosession;

            game.CurrentPlay.ElapsedTime += 0.5;
            game.CurrentPlay.Result.Add("Fumble on the play");
            if (game.CurrentPlay.PossessionChange)
            {
                game.CurrentPlay.Result.Add("Possesion changes hands");
                game.CurrentPlay.Result.Add(string.Format("{0} now has possesion", game.Possession));
            }
            else
            {
                game.CurrentPlay.Result.Add(string.Format("{0} keeps possesion", game.Possession));
            }

            return game;
        }
    }
}
