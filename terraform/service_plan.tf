resource "azurerm_service_plan" "asp" {
  name                = "asp-${var.workload}-${var.environment}-${var.location}"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name

  os_type  = "Linux"
  sku_name = "FC1"

  tags = var.tags
}
