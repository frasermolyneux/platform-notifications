using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.FuncApp.Services;

/// <summary>
/// Service for sending emails via Azure Communication Services.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Sends an email using Azure Communication Services.
    /// </summary>
    /// <param name="request">The email request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The send email response.</returns>
    Task<SendEmailResponseDto> SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default);
}
