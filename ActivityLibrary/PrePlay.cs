using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class PrePlay : CodeActivity<Game>
    {
        public InArgument<Game> Game { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            //inside here we will do things like decide the next play,
            //substitute players for the new play,
            //substitute for players that have been injured in the post play
            var game = Game.Get(context);

            //if there are 0 plays - we have a new game
            if (game.Plays.Count == 0)
            {
                game.CurrentPlay = new DomainObjects.Play()
                {
                    Down = Downs.None,
                    StartTime = 0,
                    PlayType = PlayType.Kickoff,
                    PossessionChange = false
                };
            }

            return game;
        }
    }
}
