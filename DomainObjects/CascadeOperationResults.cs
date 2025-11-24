namespace DomainObjects;

/// <summary>
/// Result of a cascade soft delete operation
/// Provides detailed information about all entities that were soft-deleted
/// </summary>
public class CascadeDeleteResult
{
    /// <summary>
    /// Total number of entities soft-deleted (including the root entity)
    /// </summary>
    public int TotalEntitiesDeleted { get; set; }

    /// <summary>
    /// Breakdown of deleted entities by type
    /// Example: { "Leagues": 1, "Conferences": 2, "Divisions": 8, "Teams": 32, "Players": 1696 }
    /// </summary>
    public Dictionary<string, int> DeletedByType { get; set; } = new();

    /// <summary>
    /// List of entity IDs that were soft-deleted, organized by type
    /// Example: { "Teams": [1, 2, 3, 4], "Players": [10, 11, 12, ...] }
    /// </summary>
    public Dictionary<string, List<int>> DeletedIds { get; set; } = new();

    /// <summary>
    /// Warnings or informational messages about the cascade operation
    /// Example: "3 games with this team are preserved (historical data)"
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Who initiated the delete operation
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Reason for the deletion
    /// </summary>
    public string? DeletionReason { get; set; }

    /// <summary>
    /// When the cascade delete was executed
    /// </summary>
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the cascade operation completed successfully
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a cascade restore operation
/// Provides detailed information about all entities that were restored
/// </summary>
public class CascadeRestoreResult
{
    /// <summary>
    /// Total number of entities restored (including the root entity)
    /// </summary>
    public int TotalEntitiesRestored { get; set; }

    /// <summary>
    /// Breakdown of restored entities by type
    /// Example: { "Leagues": 1, "Conferences": 2, "Divisions": 8, "Teams": 32 }
    /// </summary>
    public Dictionary<string, int> RestoredByType { get; set; } = new();

    /// <summary>
    /// List of entity IDs that were restored, organized by type
    /// </summary>
    public Dictionary<string, List<int>> RestoredIds { get; set; } = new();

    /// <summary>
    /// Warnings or informational messages about the restore operation
    /// Example: "100 players remain deleted (not included in cascade restore)"
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Parent entities that were auto-restored (if autoRestoreParents=true)
    /// </summary>
    public List<string> AutoRestoredParents { get; set; } = new();

    /// <summary>
    /// When the cascade restore was executed
    /// </summary>
    public DateTime RestoredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the cascade operation completed successfully
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of validating whether an entity can be restored
/// </summary>
public class RestoreValidationResult
{
    /// <summary>
    /// Whether the entity can be safely restored
    /// </summary>
    public bool CanRestore { get; set; }

    /// <summary>
    /// List of validation errors preventing restore
    /// Example: "Parent Division (ID: 5) is soft-deleted"
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// List of warnings that don't prevent restore but should be noted
    /// Example: "10 child players will remain deleted"
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Parent entities that are soft-deleted (blocking restore)
    /// </summary>
    public List<string> DeletedParents { get; set; } = new();

    /// <summary>
    /// Child entities that will remain soft-deleted after restore
    /// </summary>
    public Dictionary<string, int> OrphanedChildren { get; set; } = new();
}

/// <summary>
/// Configuration for bulk operations on soft-deleted entities
/// </summary>
public class BulkOperationRequest
{
    /// <summary>
    /// List of entity IDs to operate on
    /// </summary>
    public List<int> Ids { get; set; } = new();

    /// <summary>
    /// Reason for the bulk operation
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// For purge operations: confirmation string (must be "PERMANENT_DELETE")
    /// </summary>
    public string? Confirmation { get; set; }

    /// <summary>
    /// Whether to cascade the operation to child entities
    /// </summary>
    public bool Cascade { get; set; } = false;

    /// <summary>
    /// For restore: whether to auto-restore parent entities
    /// </summary>
    public bool AutoRestoreParents { get; set; } = false;
}

/// <summary>
/// Result of a bulk operation
/// </summary>
public class BulkOperationResult
{
    /// <summary>
    /// Total number of entities successfully processed
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Total number of entities that failed to process
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// List of entity IDs that were successfully processed
    /// </summary>
    public List<int> SuccessfulIds { get; set; } = new();

    /// <summary>
    /// List of entity IDs that failed, with error messages
    /// </summary>
    public Dictionary<int, string> FailedIds { get; set; } = new();

    /// <summary>
    /// Overall warnings about the bulk operation
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Whether the entire bulk operation completed successfully
    /// </summary>
    public bool Success { get; set; } = true;
}
