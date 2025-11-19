namespace DomainObjects.Helpers
{
    public static class GameHelper
    {
        /// <summary>
        /// Creates a new game with the provided teams
        /// </summary>
        public static Game GetNewGame(Team homeTeam, Team awayTeam)
        {
            return new Game()
            {
                HomeTeam = homeTeam,
                AwayTeam = awayTeam
            };
        }
    }
}