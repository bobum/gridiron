namespace DomainObjects;

/// <summary>
/// Base class for entities that support soft delete functionality.
/// Soft delete marks records as deleted without physically removing them from the database,
/// enabling data recovery, audit trails, and historical analysis.
/// </summary>
public abstract class SoftDeletableEntity
{
    /// <summary>
    /// Indicates whether this entity has been soft deleted.
    /// Soft deleted entities are excluded from normal queries via EF Core query filters.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// UTC timestamp when this entity was soft deleted.
    /// Null if the entity has not been deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Username or identifier of the user/system that soft deleted this entity.
    /// Null if the entity has not been deleted.
    /// Useful for audit trails and determining who performed the deletion.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Optional reason for why this entity was soft deleted.
    /// Examples: "Week rollback", "Duplicate entry", "User request", etc.
    /// Null if no reason was provided or entity has not been deleted.
    /// </summary>
    public string? DeletionReason { get; set; }

    /// <summary>
    /// Marks this entity as soft deleted with the current UTC timestamp.
    /// </summary>
    /// <param name="deletedBy">Username or identifier of who is deleting this entity</param>
    /// <param name="reason">Optional reason for the deletion</param>
    public void SoftDelete(string? deletedBy = null, string? reason = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        DeletionReason = reason;
    }

    /// <summary>
    /// Restores a soft deleted entity by clearing the deletion flags.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        DeletionReason = null;
    }
}
