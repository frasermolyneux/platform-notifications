resource "azurerm_linux_function_app" "func" {
  name = local.func_app_name

  tags = var.tags

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  service_plan_id = azurerm_service_plan.asp.id

  storage_account_name          = azurerm_storage_account.sa.name
  storage_uses_managed_identity = true

  https_only = true

  functions_extension_version = "~4"

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      use_dotnet_isolated_runtime = true
      dotnet_version              = "9.0"
    }

    cors {
      allowed_origins = ["https://portal.azure.com"]
    }

    application_insights_connection_string = azurerm_application_insights.ai.connection_string
    application_insights_key               = azurerm_application_insights.ai.instrumentation_key

    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"

    health_check_path                 = "/api/v1/health"
    health_check_eviction_time_in_min = 5
  }

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
    "ApplicationInsightsAgent_EXTENSION_VERSION"    = "~3"
    "ServiceBusConnection__fullyQualifiedNamespace" = format("%s.servicebus.windows.net", azurerm_servicebus_namespace.sb.name)
    "ACS__Endpoint"                                 = "https://${azurerm_communication_service.acs.name}.unitedkingdom.communication.azure.com"
    "StorageAccount__BlobServiceUri"                = "https://${azurerm_storage_account.sa.name}.blob.core.windows.net"

    // https://learn.microsoft.com/en-us/azure/azure-monitor/profiler/profiler-azure-functions#app-settings-for-enabling-profiler
    "APPINSIGHTS_PROFILERFEATURE_VERSION"  = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION" = "~3"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_RUN_FROM_PACKAGE"]
    ]
  }
}
