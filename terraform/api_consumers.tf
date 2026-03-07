locals {
  # Flatten consumer roles into individual assignments
  consumer_role_assignments = flatten([
    for consumer in var.api_consumers : [
      for role in consumer.roles : {
        key           = "${consumer.workload_name}-${role}"
        workload_name = consumer.workload_name
        display_name  = consumer.display_name
        role          = role
      }
    ]
  ])
}

data "azuread_service_principal" "consumers" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  display_name = each.value.display_name
}

resource "azurerm_api_management_subscription" "consumers" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  api_management_name = azurerm_api_management.apim.name
  resource_group_name = data.azurerm_resource_group.rg.name
  display_name        = each.value.workload_name
  product_id          = azurerm_api_management_product.api_product.id
  state               = "active"
}

resource "random_id" "consumer" {
  for_each    = { for c in var.api_consumers : c.workload_name => c }
  byte_length = 6

  keepers = {
    workload = each.value.workload_name
  }
}

resource "azurerm_key_vault" "consumer" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  name                = "kv-${random_id.consumer[each.key].hex}-${local.location_short}"
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  tenant_id           = data.azuread_client_config.current.tenant_id
  sku_name            = "standard"

  rbac_authorization_enabled = true

  tags = merge(var.tags, {
    consumerWorkload    = each.value.workload_name
    consumerPrincipalId = data.azuread_service_principal.consumers[each.key].object_id
  })
}

resource "azurerm_role_assignment" "consumer_kv_secrets_user" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  scope                = azurerm_key_vault.consumer[each.key].id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = data.azuread_service_principal.consumers[each.key].object_id
}

resource "azurerm_role_assignment" "consumer_kv_deploy_secrets_officer" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  scope                = azurerm_key_vault.consumer[each.key].id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azuread_client_config.current.object_id
}

resource "azurerm_key_vault_secret" "consumer_apim_key" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  name         = "${each.value.workload_name}-apim-subscription-key"
  value        = azurerm_api_management_subscription.consumers[each.key].primary_key
  key_vault_id = azurerm_key_vault.consumer[each.key].id

  depends_on = [azurerm_role_assignment.consumer_kv_deploy_secrets_officer]
}

resource "azurerm_key_vault_secret" "consumer_apim_key_secondary" {
  for_each = { for c in var.api_consumers : c.workload_name => c }

  name         = "${each.value.workload_name}-apim-subscription-key-secondary"
  value        = azurerm_api_management_subscription.consumers[each.key].secondary_key
  key_vault_id = azurerm_key_vault.consumer[each.key].id

  depends_on = [azurerm_role_assignment.consumer_kv_deploy_secrets_officer]
}

# App role assignments for API consumers - each consumer can have multiple roles
resource "azuread_app_role_assignment" "consumer_to_api" {
  for_each = { for a in local.consumer_role_assignments : a.key => a }

  app_role_id         = local.app_role_ids[each.value.role]
  principal_object_id = data.azuread_service_principal.consumers[each.value.workload_name].object_id
  resource_object_id  = azuread_service_principal.api.object_id
}
