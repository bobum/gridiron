using DomainObjects;
using DomainObjects.Helpers;
using StateLibrary.Actions;

namespace UnitTestProject1.Helpers
{
    public class TestGame
    {
        /// <summary>
        /// Gets a game object set to the first play (kickoff)
        /// </summary>
        /// <returns></returns>
        public Game GetGame()
        {
            var rng = new CryptoRandom();
            var game = GameHelper.GetNewGame();
            var prePlay = new PrePlay(rng);
            prePlay.Execute(game);
            return game;
        }
    }
}