using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class PenaltyApplication : IGameAction
    {
        public void Execute(Game game)
        {
            if (game.CurrentPlay.Penalties != null && game.CurrentPlay.Penalties.Count > 0)
            {
                foreach (var penalty in game.CurrentPlay.Penalties)
                {
                    //we have penalties - we need to sort them out and change posession, remove a score, change downs, alter the line of scrimmage, eject players, etc etc etc
                }
            }
        }
    }
}
