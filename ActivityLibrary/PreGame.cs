using System.Activities;
using DomainObjects;

namespace ActivityLibrary
{

    public sealed class PreGame : CodeActivity<Game>
    {
        public InArgument<Team> HomeTeam { get; set; }
        public InArgument<Team> AwayTeam { get; set; }

        protected override Game Execute(CodeActivityContext context)
        {
            var game = new Game
            {
                AwayTeam = AwayTeam.Get(context),
                HomeTeam = HomeTeam.Get(context)
            };
            return game;
        }
    }
}
