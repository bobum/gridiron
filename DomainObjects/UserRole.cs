namespace DomainObjects;

/// <summary>
/// Defines the roles a user can have within a league
/// </summary>
public enum UserRole
{
    /// <summary>
    /// League Commissioner - has full access to everything in the league
    /// </summary>
    Commissioner = 1,

    /// <summary>
    /// General Manager - has access to their specific team only
    /// </summary>
    GeneralManager = 2
}
