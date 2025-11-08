namespace StateLibrary.BaseClasses
{
    /// <summary>
    /// Convenience base class for skills check results that return yardage (int).
    /// Examples: yards gained on a run, air yards on a pass, sack yardage lost, etc.
    /// </summary>
    public abstract class YardageSkillsCheckResult : SkillsCheckResult<int>
    {
    }
}
