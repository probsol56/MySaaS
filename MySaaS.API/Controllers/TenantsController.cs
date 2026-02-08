using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySaaS.Application.Common.Interfaces;
using MySaaS.Application.DTOs;

namespace MySaaS.API.Controllers;

/// <summary>
/// Tenant management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires JWT authentication
public class TenantsController(ITenantService tenantService, IMapper mapper) : ControllerBase
{
    private readonly ITenantService _tenantService = tenantService;
    private readonly IMapper _mapper = mapper;

    // =====================================
    // CREATE
    // =====================================

    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TenantResponse>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = await _tenantService.CreateTenantAsync(request.Name, request.Identifier, cancellationToken);
        var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken);
        var response = _mapper.Map<TenantResponse>(tenant);

        return CreatedAtAction(nameof(GetTenantById), new { id = tenantId }, response);
    }

    // =====================================
    // READ
    // =====================================

    /// <summary>
    /// Gets all tenants.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantBasicResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TenantBasicResponse>>> GetAllTenants(CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
        return Ok(_mapper.Map<List<TenantBasicResponse>>(tenants));
    }

    /// <summary>
    /// Gets tenants with pagination.
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResponse<TenantBasicResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<TenantBasicResponse>>> GetTenantsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _tenantService.GetTenantsPagedAsync(pageNumber, pageSize, cancellationToken);

        var response = new PagedResponse<TenantBasicResponse>
        {
            Items = _mapper.Map<List<TenantBasicResponse>>(items),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantResponse>> GetTenantById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<TenantResponse>(tenant));
    }

    /// <summary>
    /// Gets a tenant by identifier (slug).
    /// </summary>
    [HttpGet("by-identifier/{identifier}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantResponse>> GetTenantByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetTenantByIdentifierAsync(identifier, cancellationToken);
        return Ok(_mapper.Map<TenantResponse>(tenant));
    }

    // =====================================
    // UPDATE
    // =====================================

    /// <summary>
    /// Updates a tenant.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantResponse>> UpdateTenant(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.UpdateTenantAsync(
            id,
            request.Name,
            request.IsActive,
            request.SubscriptionExpiresAt,
            cancellationToken);

        return Ok(_mapper.Map<TenantResponse>(tenant));
    }

    // =====================================
    // DELETE
    // =====================================

    /// <summary>
    /// Deletes a tenant (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(Guid id, CancellationToken cancellationToken)
    {
        await _tenantService.DeleteTenantAsync(id, cancellationToken);
        return NoContent();
    }

    // =====================================
    // VALIDATION
    // =====================================

    /// <summary>
    /// Checks if a tenant identifier is available.
    /// </summary>
    [HttpGet("check-identifier/{identifier}")]
    [ProducesResponseType(typeof(IdentifierCheckResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IdentifierCheckResponse>> CheckIdentifier(string identifier, CancellationToken cancellationToken)
    {
        var exists = await _tenantService.IdentifierExistsAsync(identifier, cancellationToken);
        return Ok(new IdentifierCheckResponse { Identifier = identifier, IsAvailable = !exists });
    }
}

// Helper DTOs for responses
public record PagedResponse<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public record IdentifierCheckResponse
{
    public string Identifier { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
}
