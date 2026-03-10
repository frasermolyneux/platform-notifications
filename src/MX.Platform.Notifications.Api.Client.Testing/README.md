# MX.Platform.Notifications.Api.Client.Testing

Test helpers for consumer apps: in-memory fakes of `INotificationsApiClient`, DTO factory methods, and DI extensions for integration tests.

## Installation

```bash
dotnet add package MX.Platform.Notifications.Api.Client.Testing
```

## Unit Tests

Use `FakeNotificationsApiClient` directly for lightweight unit tests:

```csharp
var fakeClient = new FakeNotificationsApiClient();
var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Welcome!");
await fakeClient.Email.SendEmail(request);

// Assert emails were sent
Assert.Single(fakeClient.EmailApi.SentEmails);
Assert.Equal("Welcome!", fakeClient.EmailApi.SentEmails.First().Subject);
```

## Integration Tests (DI)

Replace real services with fakes in your test DI container:

```csharp
services.AddFakeNotificationsApiClient();
```

Or pre-configure responses:

```csharp
services.AddFakeNotificationsApiClient(client =>
{
    client.EmailApi.WithSuccessResponse(new SendEmailResponseDto
    {
        MessageId = "test-msg-001",
        Status = "Succeeded"
    });
});
```

## Simulating Failures

Configure the fake to return error responses:

```csharp
var fakeClient = new FakeNotificationsApiClient();
fakeClient.EmailApi.WithFailure(
    statusCode: HttpStatusCode.TooManyRequests,
    errorCode: "RATE_LIMITED",
    errorMessage: "Too many requests");

var result = await fakeClient.Email.SendEmail(request);
Assert.False(result.IsSuccess);
Assert.Equal(HttpStatusCode.TooManyRequests, result.StatusCode);
```

## Factory Methods

`SendEmailRequestDtoFactory` provides methods for creating test DTOs with sensible defaults:

```csharp
// Create a request with defaults (contoso.com, "Test Email" subject, one recipient)
var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

// Override specific fields
var customRequest = SendEmailRequestDtoFactory.CreateSendEmailRequest(
    senderDomain: "myapp.com",
    subject: "Password Reset",
    htmlBody: "<p>Click here to reset your password.</p>");

// Create recipients
var recipient = SendEmailRequestDtoFactory.CreateRecipient(
    emailAddress: "user@example.com",
    displayName: "Jane Doe");

// Create response DTOs
var response = SendEmailRequestDtoFactory.CreateSendEmailResponse(
    messageId: "msg-123",
    status: "Succeeded");
```

## Resetting State Between Tests

```csharp
fakeClient.Reset(); // Clears SentEmails and resets to default success behavior
```
