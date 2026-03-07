# MX.Platform.Notifications.Api.Client.Testing

Test helpers for consumer apps: in-memory fakes of `INotificationsApiClient`, DTO factory methods, and DI extensions for integration tests.

## Installation

```bash
dotnet add package MX.Platform.Notifications.Api.Client.Testing
```

## Usage

### Unit Tests
```csharp
var fakeClient = new FakeNotificationsApiClient();
var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Welcome!");
await fakeClient.Email.SendEmail(request);
Assert.Single(fakeClient.EmailApi.SentEmails);
```

### Integration Tests (DI)
```csharp
services.AddFakeNotificationsApiClient();
```
