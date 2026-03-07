using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.Api.Client.Testing.Factories;

/// <summary>
/// Factory methods for creating notification DTOs in tests.
/// </summary>
public static class SendEmailRequestDtoFactory
{
    /// <summary>
    /// Creates a SendEmailRequestDto with the specified values.
    /// </summary>
    public static SendEmailRequestDto CreateSendEmailRequest(
        string senderDomain = "contoso.com",
        string? senderDisplayName = "Contoso Notifications",
        string senderUsername = "noreply",
        string subject = "Test Email",
        string? htmlBody = "<p>Test email body</p>",
        string? plainTextBody = "Test email body",
        List<EmailRecipientDto>? to = null,
        List<EmailRecipientDto>? cc = null,
        List<EmailRecipientDto>? bcc = null)
    {
        return new SendEmailRequestDto
        {
            SenderDomain = senderDomain,
            SenderDisplayName = senderDisplayName,
            SenderUsername = senderUsername,
            Subject = subject,
            HtmlBody = htmlBody,
            PlainTextBody = plainTextBody,
            To = to ?? [CreateRecipient()],
            Cc = cc,
            Bcc = bcc
        };
    }

    /// <summary>
    /// Creates an EmailRecipientDto with the specified values.
    /// </summary>
    public static EmailRecipientDto CreateRecipient(
        string emailAddress = "test@example.com",
        string? displayName = "Test User")
    {
        return new EmailRecipientDto
        {
            EmailAddress = emailAddress,
            DisplayName = displayName
        };
    }

    /// <summary>
    /// Creates a SendEmailResponseDto with the specified values.
    /// </summary>
    public static SendEmailResponseDto CreateSendEmailResponse(
        string messageId = "test-message-id",
        string status = "Succeeded")
    {
        return new SendEmailResponseDto
        {
            MessageId = messageId,
            Status = status
        };
    }
}
