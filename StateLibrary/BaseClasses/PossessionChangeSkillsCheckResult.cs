using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.BaseClasses
{
    public abstract class PossessionChangeSkillsCheckResult : IGameAction
    {
        public Possession Possession { get; private protected set; } = Possession.Home;
        public abstract void Execute(Game game);
    }
}