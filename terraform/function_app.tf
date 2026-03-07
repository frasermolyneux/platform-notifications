resource "azurerm_function_app_flex_consumption" "func" {
  name = local.func_app_name

  tags = var.tags

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  service_plan_id = azurerm_service_plan.asp.id

  storage_container_type      = "blobContainer"
  storage_container_endpoint  = "${azurerm_storage_account.sa.primary_blob_endpoint}${azurerm_storage_container.deployments.name}"
  storage_authentication_type = "SystemAssignedIdentity"

  runtime_name    = "dotnet-isolated"
  runtime_version = "9.0"

  identity {
    type = "SystemAssigned"
  }

  site_config {}

  auth_settings_v2 {
    auth_enabled    = true
    runtime_version = "~1"

    require_authentication = true
    unauthenticated_action = "Return401"
    excluded_paths         = ["/api/v1/health", "/api/v1/info", "/api/openapi/*"]
    require_https          = true
    http_route_api_prefix  = "/api"

    login {
      token_store_enabled = false
    }

    active_directory_v2 {
      client_id            = azuread_application.api.client_id
      tenant_auth_endpoint = "https://login.microsoftonline.com/${data.azuread_client_config.current.tenant_id}/v2.0"
      allowed_audiences    = [local.entra_api_identifier_uri]
    }
  }

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING"          = azurerm_application_insights.ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION"     = "~3"
    "ServiceBusConnection__fullyQualifiedNamespace"  = format("%s.servicebus.windows.net", azurerm_servicebus_namespace.sb.name)
    "ACS__Endpoint"                                  = "https://${azurerm_communication_service.acs.name}.unitedkingdom.communication.azure.com"
    "StorageAccount__BlobServiceUri"                 = "https://${azurerm_storage_account.sa.name}.blob.core.windows.net"
    "APPINSIGHTS_PROFILERFEATURE_VERSION"            = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION"           = "~3"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_RUN_FROM_PACKAGE"]
    ]
  }
}
