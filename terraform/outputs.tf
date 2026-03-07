output "resource_group_name" {
  value = data.azurerm_resource_group.rg.name
}

output "function_app_name" {
  value = azurerm_linux_function_app.func.name
}

output "api_management_name" {
  value = azurerm_api_management.apim.name
}

output "api_management_resource_group_name" {
  value = azurerm_api_management.apim.resource_group_name
}

output "api_version_set_id" {
  value = azurerm_api_management_api_version_set.api_version_set.name
}

output "api_management_product_id" {
  value = azurerm_api_management_product.api_product.product_id
}

output "entra_api_application_client_id" {
  value = azuread_application.api.client_id
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "storage_account_name" {
  value = azurerm_storage_account.sa.name
}
