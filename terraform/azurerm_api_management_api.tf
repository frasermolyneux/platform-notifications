resource "azurerm_api_management_logger" "app_insights" {
  name                = "${var.workload}-application-insights"
  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  application_insights {
    instrumentation_key = azurerm_application_insights.ai.instrumentation_key
  }
}

resource "azurerm_api_management_diagnostic" "app_insights" {
  identifier               = "applicationinsights"
  resource_group_name      = azurerm_api_management.apim.resource_group_name
  api_management_name      = azurerm_api_management.apim.name
  api_management_logger_id = azurerm_api_management_logger.app_insights.id

  sampling_percentage = lookup(local.app_insights_sampling_percentage, var.environment, 25)
}

# API definition — fully managed in Terraform (no CI/CD OpenAPI import)
resource "azurerm_api_management_api" "api_v1" {
  name                = "notifications-api-v1"
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  revision            = "1"
  display_name        = "Notifications API"
  path                = "notifications"
  version             = "v1"
  version_set_id      = azurerm_api_management_api_version_set.api_version_set.id
  protocols           = ["https"]
  service_url         = "https://${azurerm_function_app_flex_consumption.func.name}.azurewebsites.net/api"

  subscription_required = true
}

resource "azurerm_api_management_product_api" "api_v1" {
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  product_id          = azurerm_api_management_product.api_product.product_id
}

# --- Send Email operation (APIM → Service Bus direct) ---
resource "azurerm_api_management_api_operation" "send_email" {
  operation_id        = "send-email"
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  display_name        = "Send Email"
  method              = "POST"
  url_template        = "/v1/email/send"

  response {
    status_code = 202
    description = "Email queued for delivery"
  }
}

resource "azurerm_api_management_api_operation_policy" "send_email" {
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  operation_id        = azurerm_api_management_api_operation.send_email.operation_id

  depends_on = [
    azurerm_role_assignment.apim_to_servicebus_sender
  ]

  xml_content = <<XML
<policies>
  <inbound>
    <base />
    <set-variable name="requestBody" value="@(context.Request.Body.As&lt;string&gt;(preserveContent: true))" />
    <send-request mode="new" response-variable-name="sbResponse" timeout="10" ignore-error="true">
      <set-url>https://${azurerm_servicebus_namespace.sb.name}.servicebus.windows.net/email_send_queue/messages</set-url>
      <set-method>POST</set-method>
      <set-header name="Content-Type" exists-action="override">
        <value>application/json</value>
      </set-header>
      <authentication-managed-identity resource="https://servicebus.azure.net/" />
      <set-body>@((string)context.Variables["requestBody"])</set-body>
    </send-request>
    <choose>
      <when condition="@(context.Variables.ContainsKey(&quot;sbResponse&quot;) &amp;&amp; context.Variables[&quot;sbResponse&quot;] != null &amp;&amp; ((IResponse)context.Variables[&quot;sbResponse&quot;]).StatusCode == 201)">
        <return-response>
          <set-status code="202" reason="Accepted" />
          <set-header name="Content-Type" exists-action="override">
            <value>application/json</value>
          </set-header>
          <set-body>@{
            var trackingId = Guid.NewGuid().ToString();
            return "{\"messageId\":\"" + trackingId + "\",\"status\":\"Queued\"}";
          }</set-body>
        </return-response>
      </when>
      <otherwise>
        <trace source="send-email-policy" severity="error">
          <message>@{
            if (!context.Variables.ContainsKey("sbResponse") || context.Variables["sbResponse"] == null)
              return "Service Bus request failed (null response - network/timeout error)";
            var response = (IResponse)context.Variables["sbResponse"];
            return "Service Bus send failed. Status: " + response.StatusCode + ", Body: " + response.Body.As&lt;string&gt;();
          }</message>
        </trace>
        <return-response>
          <set-status code="500" reason="Internal Server Error" />
          <set-header name="Content-Type" exists-action="override">
            <value>application/json</value>
          </set-header>
          <set-body>{"error":"Failed to queue email for delivery"}</set-body>
        </return-response>
      </otherwise>
    </choose>
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>
XML
}

# --- Health endpoint (forwarded to Function App) ---
resource "azurerm_api_management_api_operation" "health" {
  operation_id        = "health"
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  display_name        = "Health Check"
  method              = "GET"
  url_template        = "/v1/health"

  response {
    status_code = 200
    description = "Service is healthy"
  }
}

# --- Info endpoint (forwarded to Function App) ---
resource "azurerm_api_management_api_operation" "info" {
  operation_id        = "info"
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  display_name        = "Application Info"
  method              = "GET"
  url_template        = "/v1/info"

  response {
    status_code = 200
    description = "Application version and info"
  }
}

# --- Reprocess Dead Letter Queue (forwarded to Function App) ---
resource "azurerm_api_management_api_operation" "reprocess_dlq" {
  operation_id        = "reprocess-dlq"
  api_name            = azurerm_api_management_api.api_v1.name
  api_management_name = azurerm_api_management.apim.name
  resource_group_name = azurerm_api_management.apim.resource_group_name
  display_name        = "Reprocess Dead Letter Queue"
  method              = "POST"
  url_template        = "/v1/admin/reprocess-dlq"

  response {
    status_code = 200
    description = "Dead letter queue reprocessing result"
  }
}
