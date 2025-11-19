namespace DomainObjects.Helpers
{
    public static class GameHelper
    {
        /// <summary>
        /// Get a new game with teams loaded from JSON (legacy method for backward compatibility)
        /// </summary>
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