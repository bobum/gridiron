using DomainObjects;
using System.Activities;

namespace ActivityLibrary.Plays
{
    //Kickoffs can be at the start of each half of play
    //and after every score - kick away, squib, onsides kick
    //a free kick, that is, a punt, drop kick or placekick without a tee after a safety
    public sealed class Kickoff : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = Game.Get(context);

            game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.Add("Kickoff!!");

            return game;
        }
    }
}
