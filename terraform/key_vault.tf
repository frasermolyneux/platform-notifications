resource "azurerm_key_vault" "kv" {
  name                = local.key_vault_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tenant_id           = data.azuread_client_config.current.tenant_id
  sku_name            = "standard"

  rbac_authorization_enabled = true

  tags = var.tags
}

resource "azurerm_role_assignment" "deploy_kv_secrets_officer" {
  scope                = azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_client_config.current.object_id
}
