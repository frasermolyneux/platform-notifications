using Newtonsoft.Json;

namespace MX.Platform.Notifications.Abstractions.V1.Models;

public class SendEmailRequestDto
{
    [JsonProperty("senderDomain")]
    public string SenderDomain { get; set; } = default!;

    [JsonProperty("senderDisplayName")]
    public string? SenderDisplayName { get; set; }

    [JsonProperty("senderUsername")]
    public string SenderUsername { get; set; } = "noreply";

    [JsonProperty("subject")]
    public string Subject { get; set; } = default!;

    [JsonProperty("htmlBody")]
    public string? HtmlBody { get; set; }

    [JsonProperty("plainTextBody")]
    public string? PlainTextBody { get; set; }

    [JsonProperty("to")]
    public List<EmailRecipientDto> To { get; set; } = [];

    [JsonProperty("cc")]
    public List<EmailRecipientDto>? Cc { get; set; }

    [JsonProperty("bcc")]
    public List<EmailRecipientDto>? Bcc { get; set; }
}
