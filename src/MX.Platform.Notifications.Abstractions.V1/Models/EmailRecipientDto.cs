using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

public class EmailRecipientDto
{
    [JsonProperty("emailAddress")]
    public string EmailAddress { get; set; } = default!;

    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }
}
