using MySaaS.Domain.Entities;

namespace MySaaS.Application.Common.Interfaces;

/// <summary>
/// Service for handling password reset operations.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Generates a password reset token for a user and sends reset email.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user exists and token was generated, false otherwise</returns>
    Task<bool> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets user password using a valid reset token.
    /// </summary>
    /// <param name="token">Password reset token</param>
    /// <param name="newPassword">New password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if password was reset successfully, false if token is invalid/expired</returns>
    Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
