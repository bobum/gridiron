using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for User data access
/// This is the ONLY way to access user data from the database
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their Azure AD Object ID (from JWT token)
    /// </summary>
    Task<User?> GetByAzureAdObjectIdAsync(string azureAdObjectId);

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Gets a user by ID with all their league roles loaded
    /// </summary>
    Task<User?> GetByIdWithRolesAsync(int userId);

    /// <summary>
    /// Gets a user by Azure AD Object ID with all their league roles loaded
    /// </summary>
    Task<User?> GetByAzureAdObjectIdWithRolesAsync(string azureAdObjectId);

    /// <summary>
    /// Gets all users
    /// </summary>
    Task<List<User>> GetAllAsync();

    /// <summary>
    /// Adds a new user (first-time login creates user record)
    /// </summary>
    Task<User> AddAsync(User user);

    /// <summary>
    /// Updates an existing user (e.g., LastLoginAt)
    /// </summary>
    Task UpdateAsync(User user);

    /// <summary>
    /// Gets all global administrators
    /// </summary>
    Task<List<User>> GetGlobalAdminsAsync();

    /// <summary>
    /// Checks if a user exists by Azure AD Object ID
    /// </summary>
    Task<bool> ExistsAsync(string azureAdObjectId);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}
