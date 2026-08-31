# platform-notifications agent brief

## Purpose and ownership

This repository owns the notification API contract, typed client packages, asynchronous email
processing Function App, and the Terraform for its Azure Communication Services (ACS), API
Management (APIM), Service Bus, identity, DNS records, and monitoring resources.

## Important paths

- `src/MX.Platform.Notifications.Abstractions.V1/` — public interfaces and DTOs.
- `src/MX.Platform.Notifications.Api.Client.V1/` — typed HTTP client and DI configuration.
- `src/MX.Platform.Notifications.Api.Client.Testing/` — consumer-test fakes and factories.
- `src/MX.Platform.Notifications.FuncApp/` — queue processor, health/info endpoints, DLQ
  reprocessing, and ACS email sender.
- `src/MX.Platform.Notifications.FuncApp.Tests/` — function and package tests.
- `terraform/` — application infrastructure; `backends/` and `tfvars/` contain dev/prd inputs.
- `docs/domain-setup.md` — ACS sending-domain and DNS ownership procedure.
- `version.json` — Nerdbank.GitVersioning configuration.
- `src/*/bin/`, `src/*/obj/`, and Terraform `.terraform/` directories are generated.

## Useful commands

```pwsh
dotnet build src/MX.Platform.Notifications.slnx
dotnet test src/MX.Platform.Notifications.slnx --filter "FullyQualifiedName!~IntegrationTests"
dotnet test src/MX.Platform.Notifications.FuncApp.Tests/MX.Platform.Notifications.FuncApp.Tests.csproj --filter "FullyQualifiedName~EmailProcessorFunctionTests"
dotnet format src/MX.Platform.Notifications.slnx --verify-no-changes

terraform -chdir=terraform fmt -check -recursive
terraform -chdir=terraform init -backend-config=backends/dev.backend.hcl
terraform -chdir=terraform validate
terraform -chdir=terraform plan -var-file=tfvars/dev.tfvars
```

Use the SDK pinned by `global.json`. Run the smallest test selection that covers a code change.
Terraform init, validate, or plan requires the appropriate Azure/OIDC context.

## Contracts and constraints

- The three package projects are published contracts. Preserve supported target frameworks,
  package IDs, DTO serialization, client behavior, DI entry points, and testing-fake semantics
  unless a consumer-facing change is intentional.
- APIM authenticates callers with Entra ID app roles and sends accepted email request bodies
  directly to `email_send_queue`; the Function App consumes that queue and sends through ACS.
- Preserve asynchronous `202 Queued` API behavior, queue name/message compatibility, retry and
  dead-letter behavior, and the poison-message reprocessing limit.
- Sending permission is domain-granular (`{domain}.email.sender`). Terraform owns the app roles,
  APIM policy, ACS resources, Service Bus, and ACS verification DNS records.
- DNS zones are external dependencies obtained through remote state. Follow
  `docs/domain-setup.md` before changing sending domains or apex TXT ownership.
- Terraform uses separate dev/prd Azure backends and consumes `platform-monitoring` and
  `platform-connectivity` remote state. Do not move resources between states casually.
- Deployments and NuGet releases are performed by GitHub Actions; do not deploy from routine
  development commands.

## Authoritative repository docs

- `README.md`
- `docs/domain-setup.md`
- `src/MX.Platform.Notifications.Abstractions.V1/README.md`
- `src/MX.Platform.Notifications.Api.Client.V1/README.md`
- `src/MX.Platform.Notifications.Api.Client.Testing/README.md`
