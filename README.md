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

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
