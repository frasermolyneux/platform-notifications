resource "azurerm_role_assignment" "func_to_storage" {
  scope                = azurerm_storage_account.sa.id
  role_definition_name = "Storage Blob Data Owner"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}

resource "azurerm_role_assignment" "func_to_servicebus_receiver" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}

resource "azurerm_role_assignment" "func_to_servicebus_sender" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}

resource "azurerm_role_assignment" "func_to_acs_contributor" {
  scope                = azurerm_communication_service.acs.id
  role_definition_name = "Communication and Email Service Owner"
  principal_id         = azurerm_linux_function_app.func.identity[0].principal_id
}
