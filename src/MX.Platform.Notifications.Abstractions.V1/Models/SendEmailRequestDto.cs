using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

/// <summary>
/// Request model for sending an email via the Platform Notifications service.
/// </summary>
/// <remarks>
/// <para><see cref="SenderDomain"/>, <see cref="Subject"/>, and at least one <see cref="To"/>
/// recipient are required. At least one of <see cref="HtmlBody"/> or <see cref="PlainTextBody"/>
/// should be provided.</para>
/// <para>The caller must hold the Entra ID app role <c>{domain}.email.sender</c> for the
/// specified <see cref="SenderDomain"/> (e.g. <c>xtremeidiots.com.email.sender</c>).</para>
/// </remarks>
public class SendEmailRequestDto
{
    /// <summary>
    /// <b>Required.</b> The domain to send the email from. Must be a domain that has been
    /// verified and configured in Azure Communication Services (e.g. <c>xtremeidiots.com</c>,
    /// <c>molyneux.io</c>). The caller must hold the corresponding
    /// <c>{domain}.email.sender</c> app role.
    /// </summary>
    [JsonProperty("senderDomain")]
    public string SenderDomain { get; set; } = default!;

    /// <summary>
    /// Optional display name for the sender (e.g. <c>XtremeIdiots Notifications</c>).
    /// </summary>
    [JsonProperty("senderDisplayName")]
    public string? SenderDisplayName { get; set; }

    /// <summary>
    /// The local part of the sender email address. Defaults to <c>noreply</c>, resulting in
    /// a sender address of <c>noreply@{SenderDomain}</c>.
    /// </summary>
    [JsonProperty("senderUsername")]
    public string SenderUsername { get; set; } = "noreply";

    /// <summary>
    /// <b>Required.</b> The email subject line.
    /// </summary>
    [JsonProperty("subject")]
    public string Subject { get; set; } = default!;

    /// <summary>
    /// Optional HTML body content. At least one of <see cref="HtmlBody"/> or
    /// <see cref="PlainTextBody"/> should be provided.
    /// </summary>
    [JsonProperty("htmlBody")]
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Optional plain-text body content. At least one of <see cref="HtmlBody"/> or
    /// <see cref="PlainTextBody"/> should be provided.
    /// </summary>
    [JsonProperty("plainTextBody")]
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// <b>Required.</b> Primary recipients. At least one recipient is expected.
    /// </summary>
    [JsonProperty("to")]
    public List<EmailRecipientDto> To { get; set; } = [];

    /// <summary>
    /// Optional CC (carbon copy) recipients.
    /// </summary>
    [JsonProperty("cc")]
    public List<EmailRecipientDto>? Cc { get; set; }

    /// <summary>
    /// Optional BCC (blind carbon copy) recipients.
    /// </summary>
    [JsonProperty("bcc")]
    public List<EmailRecipientDto>? Bcc { get; set; }
}
