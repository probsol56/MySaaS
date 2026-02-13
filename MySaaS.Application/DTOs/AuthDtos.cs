using System.ComponentModel.DataAnnotations;

namespace MySaaS.Application.DTOs;

// =====================================
// AUTHENTICATION DTOs
// =====================================

/// <summary>
/// Request DTO for user registration.
/// </summary>
public record RegisterRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 1)]
    public required string FirstName { get; init; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 1)]
    public required string LastName { get; init; }

    // For new tenant registration
    [Required(ErrorMessage = "Company name is required.")]
    [StringLength(100, MinimumLength = 2)]
    public required string CompanyName { get; init; }

    [Required(ErrorMessage = "Company identifier is required.")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Identifier must be lowercase letters, numbers, and hyphens only.")]
    public required string CompanyIdentifier { get; init; }
}

/// <summary>
/// Request DTO for user login.
/// </summary>
public record LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required.")]
    public required string Password { get; init; }
}

/// <summary>
/// Response DTO for successful authentication.
/// </summary>
public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserInfoResponse User { get; init; } = null!;
}

/// <summary>
/// User information included in auth response.
/// </summary>
public record UserInfoResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
}

// =====================================
// PASSWORD RESET DTOs
// =====================================

/// <summary>
/// Request DTO for forgot password.
/// </summary>
public record ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
}

/// <summary>
/// Request DTO for resetting password with token.
/// </summary>
public record ResetPasswordRequest
{
    [Required(ErrorMessage = "Reset token is required.")]
    public required string Token { get; init; }

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public required string NewPassword { get; init; }

    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public required string ConfirmPassword { get; init; }
}

/// <summary>
/// Response DTO for forgot password request.
/// </summary>
public record ForgotPasswordResponse
{
    public string Message { get; init; } = string.Empty;
}
