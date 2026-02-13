using Microsoft.Extensions.Logging;
using MySaaS.Application.Common.Interfaces;

namespace MySaaS.Infrastructure.Services;

/// <summary>
/// Placeholder email service for development.
/// TODO: Replace with actual email provider (SendGrid, AWS SES, etc.) for production.
/// </summary>
public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    private readonly ILogger<EmailService> _logger = logger;

    /// <inheritdoc/>
    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        // TODO: Replace this with actual email sending logic
        // Example providers: SendGrid, AWS SES, Mailgun, etc.

        _logger.LogInformation("=================================================");
        _logger.LogInformation("PASSWORD RESET EMAIL (Development Mode)");
        _logger.LogInformation("=================================================");
        _logger.LogInformation("To: {Email}", email);
        _logger.LogInformation("Subject: Reset Your Password");
        _logger.LogInformation("");
        _logger.LogInformation("Reset Token: {Token}", resetToken);
        _logger.LogInformation("");
        _logger.LogInformation("To reset your password, use the following token:");
        _logger.LogInformation("POST /api/auth/reset-password");
        _logger.LogInformation("{{ \"token\": \"{Token}\", \"newPassword\": \"YourNewPassword\", \"confirmPassword\": \"YourNewPassword\" }}", resetToken);
        _logger.LogInformation("=================================================");

        return Task.CompletedTask;
    }
}
