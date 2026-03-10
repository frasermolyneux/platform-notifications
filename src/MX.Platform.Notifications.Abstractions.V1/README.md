# MX.Platform.Notifications.Abstractions.V1

This package provides the abstraction layer (interfaces and models) for the Platform Notifications service.

## Installation

```bash
dotnet add package MX.Platform.Notifications.Abstractions.V1
```

## Key Types

| Type | Description |
|------|-------------|
| `INotificationsApiClient` | Unified client interface — entry point for all notification APIs |
| `ISendEmailApi` | Interface for sending emails via `SendEmail(request)` |
| `SendEmailRequestDto` | Email request — sender domain, recipients, subject, and body |
| `SendEmailResponseDto` | Email response — tracking ID and status (`Queued`) |
| `EmailRecipientDto` | Email recipient — address and optional display name |

## Required vs Optional Fields

### SendEmailRequestDto

| Field | Required | Default | Description |
|-------|----------|---------|-------------|
| `SenderDomain` | ✅ | — | Domain to send from (must be configured in ACS) |
| `Subject` | ✅ | — | Email subject line |
| `To` | ✅ | `[]` | Primary recipients (at least one expected) |
| `SenderUsername` | — | `"noreply"` | Local part of sender address |
| `SenderDisplayName` | — | `null` | Display name for the sender |
| `HtmlBody` | — | `null` | HTML body (provide at least one body) |
| `PlainTextBody` | — | `null` | Plain-text body (provide at least one body) |
| `Cc` | — | `null` | CC recipients |
| `Bcc` | — | `null` | BCC recipients |

## Usage

This package is typically consumed indirectly via the `Api.Client.V1` package. If you only need the interfaces and models (e.g. for a shared contract library), reference this package directly:

```csharp
using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Abstractions.V1.Models;

public class MyService(INotificationsApiClient notifications)
{
    public async Task SendWelcomeEmail(string recipientEmail, string recipientName)
    {
        var request = new SendEmailRequestDto
        {
            SenderDomain = "contoso.com",
            Subject = "Welcome!",
            HtmlBody = "<h1>Welcome</h1><p>Thanks for signing up.</p>",
            PlainTextBody = "Welcome! Thanks for signing up.",
            To = [new EmailRecipientDto { EmailAddress = recipientEmail, DisplayName = recipientName }]
        };

        var result = await notifications.Email.SendEmail(request);

        if (result.IsSuccess)
        {
            // result.Result.Data.MessageId — unique tracking ID
            // result.Result.Data.Status — "Queued" (delivery happens asynchronously)
        }
    }
}
```
