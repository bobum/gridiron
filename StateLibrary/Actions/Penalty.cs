using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class Penalty : IGameAction
    {
        public void Execute(Game game)
        {
            foreach (var penalty in game.CurrentPlay.Penalties)
            {
                //we have penalties - we need to sort them out and move the line of scrimmage, eject players etc etc etc
            }
        }
    }
}
