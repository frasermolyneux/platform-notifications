resource "azurerm_api_management_product" "api_product" {
  product_id = local.api_management_root_path

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  display_name = "Notifications API"

  subscription_required = false
  approval_required     = false
  published             = true
}
