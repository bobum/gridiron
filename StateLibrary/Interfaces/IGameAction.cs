using DomainObjects;
using Microsoft.Extensions.Logging;

namespace StateLibrary.Interfaces
{
    public interface IGameAction
    {
        void Execute(Game game, ILogger logger);
    }
}
