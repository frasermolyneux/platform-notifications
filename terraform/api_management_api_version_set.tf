resource "azurerm_api_management_api_version_set" "api_version_set" {
  name = local.api_management_root_path

  resource_group_name = azurerm_api_management.apim.resource_group_name
  api_management_name = azurerm_api_management.apim.name

  display_name      = "Notifications API"
  versioning_scheme = "Segment"
}
