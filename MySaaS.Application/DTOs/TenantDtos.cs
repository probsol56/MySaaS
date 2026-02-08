using System.ComponentModel.DataAnnotations;

namespace MySaaS.Application.DTOs;

// =====================================
// REQUEST DTOs (Input from client)
// =====================================

/// <summary>
/// DTO for creating a new tenant.
/// </summary>
public record CreateTenantRequest
{
    [Required(ErrorMessage = "Tenant name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tenant name must be between 2 and 100 characters.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Tenant identifier is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tenant identifier must be between 3 and 50 characters.")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Identifier must contain only lowercase letters, numbers, and hyphens.")]
    public required string Identifier { get; init; }
}

/// <summary>
/// DTO for updating an existing tenant.
/// </summary>
public record UpdateTenantRequest
{
    [Required(ErrorMessage = "Tenant name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tenant name must be between 2 and 100 characters.")]
    public required string Name { get; init; }

    public bool IsActive { get; init; } = true;

    public DateTime? SubscriptionExpiresAt { get; init; }
}

// =====================================
// RESPONSE DTOs (Output to client)
// =====================================

/// <summary>
/// DTO for tenant response.
/// </summary>
public record TenantResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? SubscriptionExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Simple tenant info DTO (for lists/dropdowns).
/// </summary>
public record TenantBasicResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
