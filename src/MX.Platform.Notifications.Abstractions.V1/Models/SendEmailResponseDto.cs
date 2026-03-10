using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

/// <summary>
/// Response model returned after submitting an email send request.
/// </summary>
/// <remarks>
/// The API returns immediately after the message is queued to Service Bus.
/// The actual email delivery happens asynchronously via Azure Communication Services.
/// </remarks>
public class SendEmailResponseDto
{
    /// <summary>
    /// A unique tracking identifier for the queued email request.
    /// </summary>
    [JsonProperty("messageId")]
    public string MessageId { get; set; } = default!;

    /// <summary>
    /// The status of the email request. Returns <c>Queued</c> when the message
    /// has been accepted for asynchronous processing.
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; } = default!;
}
