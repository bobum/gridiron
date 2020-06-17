namespace DomainObjects.Helpers
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

            return newGame;
        }
    }
}