namespace MySaaS.Domain.Common;

public class BaseEntity
{
    // Let EF Core generate the ID
    public Guid Id { get; set; }

    // Audit fields - set in SaveChangesAsync
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Soft delete support
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}