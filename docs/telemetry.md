# Function App telemetry

The Function App has two Application Insights telemetry pipelines:

- The Azure Functions host emits invocation lifecycle traces, request records,
  exceptions, and runtime warnings such as trigger or poison-message diagnostics.
- The .NET isolated worker sends application logs, custom telemetry, and audit
  events directly to Application Insights from `Program.cs`.

Worker telemetry configuration does not control host-originated telemetry. Host
volume is controlled in `host.json` with deterministic category filters:

```json
"logLevel": {
  "Function": "Warning",
  "Host.Results": "Error"
}
```

`Function = Warning` suppresses successful function start and completion traces,
which the Functions host emits at `Information`, while retaining warning, error,
and exception traces. Runtime diagnostics for retries, poison messages, and
dead-letter behavior therefore remain available.

`Host.Results = Error` suppresses successful invocation request records while
retaining request records for failed function executions.

Host sampling is disabled so retention is determined by severity rather than
probabilistic sampling. The isolated worker pipeline, including application
logging, auditing, and custom telemetry, is unchanged.

The Terraform monitoring alert uses the Service Bus `DeadletteredMessages` metric
and does not depend on successful Function App request records.

References:

- [Configure Azure Functions monitoring categories and log levels](https://learn.microsoft.com/azure/azure-functions/configure-monitoring#configure-categories)
- [Configure Azure Functions host settings](https://learn.microsoft.com/azure/azure-functions/functions-host-json#applicationinsights)
- [.NET isolated worker Application Insights](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide#application-insights)
