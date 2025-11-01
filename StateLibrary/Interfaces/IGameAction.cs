using DomainObjects;

namespace StateLibrary.Interfaces
{
    public interface IGameAction
    {
        void Execute(Game game);
    }
}
