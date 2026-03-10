using MX.Api.Abstractions;
using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.Abstractions.V1.Interfaces;

/// <summary>
/// Interface for sending emails via the Platform Notifications service.
/// </summary>
public interface ISendEmailApi
{
    /// <summary>
    /// Submits an email for asynchronous delivery via Azure Communication Services.
    /// The request is queued to Service Bus and processed in the background; the response
    /// contains a tracking ID and a status of <c>Queued</c>.
    /// </summary>
    /// <param name="request">
    /// The email request containing sender, recipients, subject, and body.
    /// <see cref="SendEmailRequestDto.SenderDomain"/> and <see cref="SendEmailRequestDto.Subject"/>
    /// are required. At least one recipient in <see cref="SendEmailRequestDto.To"/> is expected.
    /// </param>
    /// <returns>
    /// An <see cref="IApiResult{T}"/> wrapping a <see cref="SendEmailResponseDto"/>.
    /// On success, <c>IsSuccess</c> is <c>true</c> and <c>Result.Data</c> contains the tracking ID
    /// and status. On failure, <c>IsSuccess</c> is <c>false</c> and <c>Result.Errors</c> contains
    /// error details.
    /// </returns>
    Task<IApiResult<SendEmailResponseDto>> SendEmail(SendEmailRequestDto request);
}
