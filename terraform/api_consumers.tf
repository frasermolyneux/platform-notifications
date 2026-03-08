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

# App role assignments for API consumers - each consumer can have multiple roles
# Authentication is purely via Entra ID JWT with app role claims (no subscription keys)
resource "azuread_app_role_assignment" "consumer_to_api" {
  for_each = { for a in local.consumer_role_assignments : a.key => a }

  app_role_id         = local.app_role_ids[each.value.role]
  principal_object_id = data.azuread_service_principal.consumers[each.value.workload_name].object_id
  resource_object_id  = azuread_service_principal.api.object_id
}
