# MX.Platform.Notifications.Api.Client.V1

This package provides a typed HTTP client for the Platform Notifications service with Entra ID authentication and DI registration.

## Installation

```bash
dotnet add package MX.Platform.Notifications.Api.Client.V1
```

## Registration

```csharp
services.AddNotificationsApiClient(options => options
    .WithBaseUrl("https://your-notifications-api.azurewebsites.net")
    .WithEntraIdAuthentication("api://your-app-id"));
```

## Sending an Email

```csharp
public class OrderService(INotificationsApiClient notifications)
{
    public async Task SendOrderConfirmation(string customerEmail, string orderId)
    {
        var request = new SendEmailRequestDto
        {
            SenderDomain = "contoso.com",
            SenderDisplayName = "Contoso Orders",
            Subject = $"Order {orderId} Confirmed",
            HtmlBody = $"<p>Your order <strong>{orderId}</strong> has been confirmed.</p>",
            PlainTextBody = $"Your order {orderId} has been confirmed.",
            To = [new EmailRecipientDto { EmailAddress = customerEmail }]
        };

        var result = await notifications.Email.SendEmail(request);

        if (!result.IsSuccess)
        {
            // Handle failure — check result.Result.Errors for details
            throw new InvalidOperationException(
                $"Failed to send email: {result.StatusCode}");
        }

        // result.Result.Data.MessageId contains the tracking ID
        // result.Result.Data.Status will be "Queued"
    }
}
```

## Error Handling

| Status Code | Meaning |
|-------------|---------|
| 202 | Email accepted and queued for asynchronous delivery |
| 400 | Invalid request (missing required fields) |
| 401 | Authentication failure (invalid or missing token) |
| 403 | Missing required app role for the sender domain |
| 500 | Internal server error (returned as `CLIENT_ERROR` in the client) |

On unhandled exceptions (network errors, timeouts), the client returns an `IApiResult` with `IsSuccess = false`, `StatusCode = 500`, and error code `CLIENT_ERROR`. `OperationCanceledException` is not caught and will propagate.
