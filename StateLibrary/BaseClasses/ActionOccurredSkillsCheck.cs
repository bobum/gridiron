using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.BaseClasses
{
    public abstract class ActionOccurredSkillsCheck : IGameAction
    {
        public bool Occurred { get; private protected set; } = false;

        /// <summary>
        /// Represents the margin of success or failure for narrative purposes.
        /// Positive values indicate decisive success, negative values indicate decisive failure.
        /// Zero indicates an even matchup or that margin was not calculated.
        /// </summary>
        public double Margin { get; protected set; } = 0.0;

        public abstract void Execute(Game game);
    }
}
