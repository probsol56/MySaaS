using MySaaS.Application.Common.Interfaces;
using MySaaS.Domain.Entities;
using MySaaS.Infrastructure.Persistence;

namespace MySaaS.Infrastructure.Services;

/// <summary>
/// Tenant service implementation using Repository pattern.
/// Follows .NET 10 standards with proper async/await and validation.
/// </summary>
public class TenantService(IRepository<Tenant> tenantRepository, ApplicationDbContext context) : ITenantService
{
    private readonly IRepository<Tenant> _tenantRepository = tenantRepository;
    private readonly ApplicationDbContext _context = context;

    // =====================================
    // CREATE
    // =====================================

    public async Task<Guid> CreateTenantAsync(string name, string identifier, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Tenant identifier is required.", nameof(identifier));

        // Check for duplicate identifier
        if (await IdentifierExistsAsync(identifier, cancellationToken))
            throw new InvalidOperationException($"A tenant with identifier '{identifier}' already exists.");

        // Create new tenant
        var tenant = new Tenant
        {
            Name = name,
            Identifier = identifier.ToLowerInvariant(),
            IsActive = true
        };

        await _tenantRepository.AddAsync(tenant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }

    // =====================================
    // READ
    // =====================================

    public async Task<Tenant> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID '{id}' not found.");
        return tenant;
    }

    public async Task<Tenant> GetTenantByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        var tenant = await _tenantRepository.FirstOrDefaultAsync(t => t.Name == name, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with name '{name}' not found.");
        return tenant;
    }

    public async Task<Tenant> GetTenantByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Tenant identifier is required.", nameof(identifier));

        var tenant = await _tenantRepository.FirstOrDefaultAsync(
            t => t.Identifier == identifier.ToLower(), cancellationToken) ?? throw new KeyNotFoundException($"Tenant with identifier '{identifier}' not found.");
        return tenant;
    }

    public async Task<List<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.GetAllAsync(cancellationToken);
    }

    public async Task<(List<Tenant> Items, int TotalCount)> GetTenantsPagedAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Max limit

        return await _tenantRepository.GetPagedAsync(pageNumber, pageSize, null, cancellationToken);
    }

    // =====================================
    // UPDATE
    // =====================================

    public async Task<Tenant> UpdateTenantAsync(
        Guid id,
        string name,
        bool isActive,
        DateTime? subscriptionExpiresAt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID '{id}' not found.");

        // Update fields
        tenant.Name = name;
        tenant.IsActive = isActive;
        tenant.SubscriptionExpiresAt = subscriptionExpiresAt;

        _tenantRepository.Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        return tenant;
    }

    // =====================================
    // DELETE (Soft Delete)
    // =====================================

    public async Task DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID '{id}' not found.");
        _tenantRepository.Remove(tenant);
        await _context.SaveChangesAsync(cancellationToken); // Soft delete handled by SaveChangesAsync
    }

    // =====================================
    // VALIDATION
    // =====================================

    public async Task<bool> IdentifierExistsAsync(string identifier, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.AnyAsync(
            t => t.Identifier == identifier.ToLower(), cancellationToken);
    }
}
