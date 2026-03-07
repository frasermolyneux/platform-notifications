resource "azuread_application" "api" {
  display_name     = local.entra_api_app_display_name
  description      = "Platform Notifications API"
  sign_in_audience = "AzureADMyOrg"

  identifier_uris = [local.entra_api_identifier_uri]

  dynamic "app_role" {
    for_each = local.app_roles
    content {
      allowed_member_types = ["Application"]
      description          = app_role.value.display_name
      display_name         = app_role.value.display_name
      id                   = app_role.value.id
      value                = app_role.value.value
      enabled              = true
    }
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }

  prevent_duplicate_names = true
}

resource "azuread_service_principal" "api" {
  client_id                    = azuread_application.api.client_id
  app_role_assignment_required = false

  owners = [
    data.azuread_client_config.current.object_id
  ]
}
