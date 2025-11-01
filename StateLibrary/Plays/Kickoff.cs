using DomainObjects;
using Microsoft.Extensions.Logging;
using StateLibrary.Interfaces;

namespace StateLibrary.Plays
{
    //Kickoffs can be at the start of each half of play
    //and after every score - kick away, squib, onsides kick
    //a free kick, that is, a punt, drop kick or placekick without a tee after a safety
    public sealed class Kickoff : IGameAction
    {
        public void Execute(Game game)
        {
           game.CurrentPlay.ElapsedTime += 6.5;
            game.CurrentPlay.Result.LogInformation("Kickoff!!");
        }
    }
}
