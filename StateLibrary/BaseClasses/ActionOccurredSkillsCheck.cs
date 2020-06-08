using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.BaseClasses
{
    public abstract class ActionOccurredSkillsCheck : IGameAction
    {
        public bool Occurred { get; private protected set; } = false;
        public abstract void Execute(Game game);
    }
}
