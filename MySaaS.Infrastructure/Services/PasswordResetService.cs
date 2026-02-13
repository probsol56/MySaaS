using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MySaaS.Application.Common.Interfaces;
using MySaaS.Domain.Entities;

namespace MySaaS.Infrastructure.Services;

/// <summary>
/// Service for handling password reset operations.
/// </summary>
public sealed class PasswordResetService(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    ILogger<PasswordResetService> logger) : IPasswordResetService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<PasswordResetService> _logger = logger;

    // Token expiration time (1 hour)
    private const int TokenExpirationHours = 1;

    /// <inheritdoc/>
    public async Task<bool> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist (security best practice)
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            return false;
        }

        // Generate a cryptographically secure random token
        var token = GenerateSecureToken();

        // Store token and expiration in database
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(TokenExpirationHours);

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to update user with password reset token for email: {Email}", email);
            return false;
        }

        // Send password reset email
        await _emailService.SendPasswordResetEmailAsync(email, token, cancellationToken);

        _logger.LogInformation("Password reset token generated for user: {Email}", email);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        // Find user by reset token
        var user = (await _userManager.GetUsersForClaimAsync(new System.Security.Claims.Claim("dummy", "dummy")))
            .FirstOrDefault();

        // Since we can't query by custom properties directly with UserManager,
        // we need to use a workaround or query the database directly
        // For now, let's iterate through all users (not ideal for production with many users)
        var allUsers = _userManager.Users.Where(u => u.PasswordResetToken == token).ToList();
        user = allUsers.FirstOrDefault();

        if (user == null)
        {
            _logger.LogWarning("Invalid password reset token provided");
            return false;
        }

        // Check if token has expired
        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired password reset token used for user: {Email}", user.Email);
            return false;
        }

        // Reset the password
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to reset password for user: {Email}. Errors: {Errors}",
                user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return false;
        }

        // Clear the reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Password successfully reset for user: {Email}", user.Email);
        return true;
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private static string GenerateSecureToken()
    {
        // Generate 32 random bytes and convert to base64 string
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
