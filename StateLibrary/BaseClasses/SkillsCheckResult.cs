using DomainObjects;
using StateLibrary.Interfaces;

namespace StateLibrary.BaseClasses
{
    /// <summary>
    /// Base class for skills check results that return a typed value.
    /// Use this when a skills check determines not just IF something happened,
    /// but also WHAT the outcome was (e.g., yards gained, player selected).
    /// </summary>
    /// <typeparam name="T">The type of result this skills check returns</typeparam>
    public abstract class SkillsCheckResult<T> : IGameAction
    {
        /// <summary>
        /// The result of the skills check calculation.
        /// </summary>
        public T Result { get; protected set; }

        public abstract void Execute(Game game);
    }
}
