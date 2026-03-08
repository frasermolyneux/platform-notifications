output "resource_group_name" {
  value = data.azurerm_resource_group.rg.name
}

output "function_app_name" {
  value = azurerm_function_app_flex_consumption.func.name
}

output "api_management_name" {
  value = azurerm_api_management.apim.name
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
