using DomainObjects;

namespace StateLibrary.BaseClasses
{
    /// <summary>
    /// Convenience base class for skills check results that determine possession changes.
    /// Examples: fumble recovery, interception, onside kick recovery, etc.
    /// </summary>
    public abstract class PossessionChangeSkillsCheckResult : SkillsCheckResult<Possession>
    {
        /// <summary>
        /// Alias for Result property to maintain backward compatibility.
        /// Use this property to get/set which team has possession after the event.
        /// </summary>
        public Possession Possession
        {
            get => Result;
            protected set => Result = value;
        }
    }
}