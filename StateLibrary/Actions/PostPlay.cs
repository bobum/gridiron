using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    class PostPlay : IGameAction
    {
        public void Execute(Game game)
        {
            //inside here we will do things like check for injuries, advance the down, change possession
            //determine if it's a hurry up offense or if they are trying to
            //kill the clock and add time appropriately...
            //add the current play to the plays list
            game.Plays.Add(game.CurrentPlay);
        }
    }
}
