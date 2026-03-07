# MX.Platform.Notifications.Api.Client.V1

This package provides a web service client for the Platform Notifications service.

## Installation

```bash
dotnet add package MX.Platform.Notifications.Api.Client.V1
```

## Usage

```csharp
services.AddNotificationsApiClient(options => options
    .WithBaseUrl("https://your-notifications-api.azurewebsites.net")
    .WithEntraIdAuthentication("api://your-app-id"));
```
