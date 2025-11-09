using DomainObjects;

namespace StateLibrary.BaseClasses
{
    /// <summary>
    /// Convenience base class for skills check results that return a Player.
    /// Examples: receiver selection, tackler selection, sacker selection, etc.
    /// </summary>
    public abstract class PlayerSelectionSkillsCheckResult : SkillsCheckResult<Player>
    {
    }
}
