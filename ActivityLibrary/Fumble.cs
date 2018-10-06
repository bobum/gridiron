using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class Fumble : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            CryptoRandom rng = new CryptoRandom();

            //was there a fumble? Totally random for now...
            var fumble = rng.Next(2);
            if (fumble == 1)
            {
                //for now this is a totally random determination of who gets
                //the ball after a fumble and we change the possestion of the game
                var toss = rng.Next(2);
                var preFumble = game.Posession;
                game.Posession = toss == 1 ? Posession.Away : Posession.Home;
                game.CurrentPlay.PossessionChange = preFumble != game.Posession;
                game.CurrentPlay.ElapsedTime += 0.5;
                game.CurrentPlay.Result.Add("Fumble on the play");
                if (game.CurrentPlay.PossessionChange)
                {
                    game.CurrentPlay.Result.Add("Possesion changes hands");
                }
                game.CurrentPlay.Result.Add(string.Format("{0} has possesion", game.Posession));
            }           
            
            return game;
        }
    }
}
