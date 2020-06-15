using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class Penalty : IGameAction
    {
        public void Execute(Game game)
        {
            if (game.CurrentPlay.Penalties.Count > 0)
            {
                //we have penalties - we need to sort them out and move the line of scrimmage, eject players etc etc etc
            }
        }
    }
}
