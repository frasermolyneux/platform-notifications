using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

public class SendEmailResponseDto
{
    [JsonProperty("messageId")]
    public string MessageId { get; set; } = default!;

    [JsonProperty("status")]
    public string Status { get; set; } = default!;
}
