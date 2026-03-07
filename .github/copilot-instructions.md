# Copilot Instructions

## Architecture
- .NET 9 solution in `src/MX.Platform.Notifications.sln` with a Function App (`MX.Platform.Notifications.FuncApp`) plus abstractions, a typed API client, and a testing package.
- Three NuGet packages are published: `MX.Platform.Notifications.Abstractions.V1` (interfaces/DTOs), `MX.Platform.Notifications.Api.Client.V1` (typed HTTP client), and `MX.Platform.Notifications.Api.Client.Testing` (in-memory fakes and DTO factories for consumer test projects).
- Function App uses Azure Communication Services to send emails with domain-granular permissions via Entra ID app roles.
- HTTP triggers handle email submission and DLQ reprocessing; Service Bus trigger handles queue processing with Polly retries.
- API security is Entra ID via Easy Auth v2; domain-specific app roles (e.g. `xtremeidiots.com.email.sender`) control which callers can send from which domains.
- Build versioning uses Nerdbank.GitVersioning (`version.json` at repo root).

## Workflows
- Build: `dotnet build src/MX.Platform.Notifications.sln`
- Test: `dotnet test src/MX.Platform.Notifications.sln`
- Test framework: xUnit + Moq + native assertions.

## Infrastructure
- Terraform under `terraform/` provisions ACS, Function App (Flex Consumption), Service Bus (Basic), APIM (Consumption), Entra ID app registration, DNS records (Azure DNS + Cloudflare), Application Insights, and monitoring alerts.
- GitHub Actions workflows cover build/test, codequality, PR verify, deploy-dev/prd, destroy-environment, dependabot-automerge, and NuGet publishing.
