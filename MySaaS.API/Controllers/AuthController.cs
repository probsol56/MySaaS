using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySaaS.Application.Common.Interfaces;
using MySaaS.Application.Common.Models;
using MySaaS.Application.DTOs;
using MySaaS.Domain.Entities;
using Microsoft.Extensions.Options;
using MySaaS.Infrastructure.Persistence;

namespace MySaaS.API.Controllers;

/// <summary>
/// Authentication endpoints for user registration and login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<ApplicationUser> userManager,
SignInManager<ApplicationUser> signInManager,
ITokenService tokenService,
ITenantService tenantService,
ApplicationDbContext context,
IOptions<JwtSettings> jwtSettings) : ControllerBase
{

    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ITenantService _tenantService = tenantService;
    private readonly ApplicationDbContext _context = context;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;




    /// <summary>
    /// Registers a new user and creates their tenant (company).
    /// Uses a database transaction to ensure atomicity.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Conflict(new { message = "Email is already registered." });
        }

        // Check if company identifier exists
        if (await _tenantService.IdentifierExistsAsync(request.CompanyIdentifier, cancellationToken))
        {
            return Conflict(new { message = "Company identifier is already taken." });
        }

        // Use explicit transaction for atomicity
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create tenant
            var tenantId = await _tenantService.CreateTenantAsync(
                request.CompanyName,
                request.CompanyIdentifier,
                cancellationToken);

            // Create user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = tenantId
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // Transaction will automatically rollback
                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            // Commit transaction - both tenant and user created successfully
            await transaction.CommitAsync(cancellationToken);

            // Generate tokens
            var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken);
            var response = GenerateAuthResponse(user, tenant);

            return CreatedAtAction(nameof(Register), response);
        }
        catch
        {
            // Transaction will automatically rollback on any exception
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }


    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Unauthorized(new { message = "Account is locked. Please try again later." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        // Get tenant info
        var tenant = await _tenantService.GetTenantByIdAsync(user.TenantId, cancellationToken);

        // Check if tenant is active
        if (!tenant.IsActive)
        {
            return Unauthorized(new { message = "Your account has been deactivated." });
        }

        // Generate tokens
        var response = GenerateAuthResponse(user, tenant);

        return Ok(response);
    }

    /// <summary>
    /// Gets current user info (requires authentication).
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var tenant = await _tenantService.GetTenantByIdAsync(user.TenantId, cancellationToken);

        return Ok(new UserInfoResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            TenantName = tenant.Name
        });
    }

    private AuthResponse GenerateAuthResponse(ApplicationUser user, Tenant tenant)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                TenantName = tenant.Name
            }
        };
    }

    /// <summary>
    /// Initiates password reset process by sending a reset token to the user's email.
    /// Returns success message regardless of whether email exists (security best practice).
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IPasswordResetService passwordResetService,
        CancellationToken cancellationToken)
    {
        // Generate reset token and send email
        await passwordResetService.GeneratePasswordResetTokenAsync(request.Email, cancellationToken);

        // Always return success message (don't reveal if email exists)
        return Ok(new ForgotPasswordResponse
        {
            Message = "If an account with that email exists, a password reset link has been sent."
        });
    }

    /// <summary>
    /// Resets user password using a valid reset token.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] IPasswordResetService passwordResetService,
        CancellationToken cancellationToken)
    {
        var success = await passwordResetService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            cancellationToken);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired reset token." });
        }

        return Ok(new { message = "Password has been reset successfully." });
    }
}

