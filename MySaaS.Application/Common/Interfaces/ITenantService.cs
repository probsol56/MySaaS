using MySaaS.Domain.Entities;

namespace MySaaS.Application.Common.Interfaces;

/// <summary>
/// Service interface for tenant management operations.
/// </summary>
public interface ITenantService
{
    // Create
    Task<Guid> CreateTenantAsync(string name, string identifier, CancellationToken cancellationToken = default);

    // Read
    Task<Tenant> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant> GetTenantByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Tenant> GetTenantByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    Task<List<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    Task<(List<Tenant> Items, int TotalCount)> GetTenantsPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    // Update
    Task<Tenant> UpdateTenantAsync(Guid id, string name, bool isActive, DateTime? subscriptionExpiresAt, CancellationToken cancellationToken = default);

    // Delete (soft delete)
    Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default);

    // Validation
    Task<bool> IdentifierExistsAsync(string identifier, CancellationToken cancellationToken = default);
}
