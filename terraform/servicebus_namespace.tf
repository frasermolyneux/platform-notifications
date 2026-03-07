resource "azurerm_servicebus_namespace" "sb" {
  name = local.service_bus_namespace_name

  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location

  tags = var.tags

  sku = "Basic"

  local_auth_enabled = false
}
