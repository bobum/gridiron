using DomainObjects;
using System.Activities;
using ActivityLibrary;

namespace UnitTestProject.Helpers
{
    public static class GameHelper
    {
        public static Game GetNewGame()
        {
            var teams = new Teams();

            Game newGame = new Game()
            {
                HomeTeam = teams.HomeTeam,
                AwayTeam = teams.VisitorTeam
            };

            var prePlayActivity = new PrePlay
            {
                Game = new InArgument<Game>((ctx) => newGame)
            };

            return WorkflowInvoker.Invoke<Game>(prePlayActivity);
        }
    }
}