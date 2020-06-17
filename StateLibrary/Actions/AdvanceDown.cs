using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.Actions
{
    public class AdvanceDown : IGameAction
    {
        public void Execute(Game game)
        {
            //in here we'll have to check to see if any of the penalties result in loss of down
        }
    }
}
