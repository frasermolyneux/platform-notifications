resource "azurerm_api_management" "apim" {
  name                = local.api_management_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  publisher_name      = "Molyneux.IO"
  publisher_email     = "admin@molyneux.io"
  sku_name            = "Consumption_0"

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}
