using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

/// <summary>
/// Represents an email recipient with an address and optional display name.
/// </summary>
public class EmailRecipientDto
{
    /// <summary>
    /// <b>Required.</b> The recipient's email address (e.g. <c>user@example.com</c>).
    /// </summary>
    [JsonProperty("emailAddress")]
    public string EmailAddress { get; set; } = default!;

    /// <summary>
    /// Optional display name for the recipient (e.g. <c>John Smith</c>).
    /// </summary>
    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
}
