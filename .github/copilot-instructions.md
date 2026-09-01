# Copilot instructions

- This repository is the source of truth for the notification abstractions, typed API client,
  consumer testing package, email-processing Function App, and their Terraform.
- Use the SDK pinned in `global.json`; the Function App targets .NET 10 and the published libraries
  target .NET 9 and .NET 10.
- Keep the abstractions, client, testing, and Function App separation. Treat published DTOs,
  interfaces, package IDs, DI APIs, and serialized queue messages as consumer contracts.
- APIM validates domain-specific Entra ID app roles and queues email requests directly to
  `email_send_queue`. The Function App processes the queue through ACS; preserve the asynchronous
  `202 Queued`, retry, DLQ, and reprocessing semantics.
- Terraform owns ACS, APIM, Service Bus, Entra application roles, Function App, DNS verification
  records, Application Insights, and alerts. It uses dev/prd backends plus monitoring and
  connectivity remote state.
- Follow `docs/domain-setup.md` for sending-domain or DNS ownership changes.
- Build and test through `src/MX.Platform.Notifications.slnx`; use targeted tests where possible.
- Never commit credentials or generated `bin/`, `obj/`, `.terraform/`, or state files.
