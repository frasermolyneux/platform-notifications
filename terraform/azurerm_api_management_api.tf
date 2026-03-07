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
