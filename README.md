# Platform Notifications

[![Code Quality](https://github.com/frasermolyneux/platform-notifications/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/platform-notifications/actions/workflows/codequality.yml)
[![Build and Test](https://github.com/frasermolyneux/platform-notifications/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/platform-notifications/actions/workflows/build-and-test.yml)
[![Deploy Dev](https://github.com/frasermolyneux/platform-notifications/actions/workflows/deploy-dev.yml/badge.svg)](https://github.com/frasermolyneux/platform-notifications/actions/workflows/deploy-dev.yml)
[![Deploy Prd](https://github.com/frasermolyneux/platform-notifications/actions/workflows/deploy-prd.yml/badge.svg)](https://github.com/frasermolyneux/platform-notifications/actions/workflows/deploy-prd.yml)

## Documentation

* [Architecture Overview](/docs/architecture.md) - High level architecture and component design
* [API Versioning and APIM](/docs/api-versioning-and-apim.md) - API versioning strategy and APIM routing
* [Domain Setup](/docs/domain-setup.md) - Guide for adding new sending domains
* [Development Workflows](/docs/development-workflows.md) - Branch strategy, CI/CD triggers, and deployment flows

## Overview

Platform Notifications is a centralised .NET 9 email notification service backed by Azure Communication Services that provides domain-granular sending across six custom domains (xtremeidiots.com, molyneux.io, molyneux.me, molyneux.dev, geo-location.net, craftpledge.org). The API is fronted by an APIM Consumption instance with Entra ID app role authorization, and messages flow through a Service Bus queue for resilient delivery with Polly retries and dead-letter handling. Infrastructure is managed by Terraform and deployed via GitHub Actions workflows above.

## NuGet Packages

| Package | Description |
|---|---|
| `MX.Platform.Notifications.Abstractions.V1` | Interfaces and DTOs for the Notifications API |
| `MX.Platform.Notifications.Api.Client.V1` | Typed HTTP client with DI registration via `AddNotificationsApiClient()` |
| `MX.Platform.Notifications.Api.Client.Testing` | In-memory fakes and DTO factory helpers for consumer test projects |

## Quick Start

### 1. Register the client

```csharp
// In your Program.cs or Startup.cs
services.AddNotificationsApiClient(options => options
    .WithBaseUrl("https://apim-platform-notifications-prd-uksouth.azure-api.net/notifications")
    .WithEntraIdAuthentication("api://{tenant-id}/platform-notifications-api-prd"));
```

### 2. Send an email

```csharp
public class MyService(INotificationsApiClient notifications)
{
    public async Task NotifyUser(string email, string name)
    {
        var request = new SendEmailRequestDto
        {
            SenderDomain = "contoso.com",        // Must hold {domain}.email.sender app role
            Subject = "Your report is ready",
            HtmlBody = $"<p>Hi {name}, your report is ready to download.</p>",
            PlainTextBody = $"Hi {name}, your report is ready to download.",
            To = [new EmailRecipientDto { EmailAddress = email, DisplayName = name }]
        };

        var result = await notifications.Email.SendEmail(request);

        if (!result.IsSuccess)
        {
            // result.StatusCode and result.Result.Errors contain failure details
            throw new InvalidOperationException($"Email send failed: {result.StatusCode}");
        }

        // result.Result.Data.MessageId — unique tracking ID
        // result.Result.Data.Status — "Queued" (delivery happens asynchronously)
    }
}
```

### 3. Test with fakes

```csharp
// Replace the real client in your test DI container
services.AddFakeNotificationsApiClient();

// Or use directly in unit tests
var fakeClient = new FakeNotificationsApiClient();
await fakeClient.Email.SendEmail(SendEmailRequestDtoFactory.CreateSendEmailRequest());
Assert.Single(fakeClient.EmailApi.SentEmails);
```

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
