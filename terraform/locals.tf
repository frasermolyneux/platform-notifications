locals {
  resource_group_name = "rg-${var.workload}-${var.environment}-${var.location}"
  location_short      = substr(var.location, 0, 3)

  platform_monitoring_workspace_id = data.terraform_remote_state.platform_monitoring.outputs.log_analytics.id

  app_insights_name = "ai-${var.workload}-${var.environment}-${var.location}"

  func_app_name        = substr("fn-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}", 0, 60)
  storage_account_name = lower("sanotif${var.environment}${random_id.storage.hex}")

  api_management_name      = substr("apim-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}", 0, 50)
  api_management_root_path = "notifications-api"

  service_bus_namespace_name = substr("sb-${var.workload}-${var.environment}-${var.location}-${random_id.environment_id.hex}", 0, 50)

  entra_api_app_display_name = "platform-notifications-api-${var.environment}"
  entra_api_identifier_uri   = format("api://%s/%s", data.azuread_client_config.current.tenant_id, local.entra_api_app_display_name)

  # App role definitions for domain-granular permissions
  app_roles = concat(
    [for domain in var.sending_domains : {
      display_name = "Send emails from ${domain.name}"
      value        = "${domain.name}.email.sender"
      id           = uuidv5("dns", "${domain.name}.email.sender")
    }],
    [{
      display_name = "Reprocess dead letter queues"
      value        = "admin.deadletter"
      id           = uuidv5("dns", "admin.deadletter")
    }]
  )

  app_role_ids = { for role in local.app_roles : role.value => role.id }

  # Split domains by DNS provider
  azure_dns_domains  = [for d in var.sending_domains : d if d.dns_provider == "azure"]
  cloudflare_domains = [for d in var.sending_domains : d if d.dns_provider == "cloudflare"]

  # Pre-existing apex TXT values migrated from platform-connectivity.
  # azurerm_dns_txt_record manages the entire record set at a given name, so
  # any values not listed here will be REMOVED on apply. When onboarding a new
  # Azure DNS domain, check the zone for existing apex TXT records and add them
  # below. Domains with no pre-existing values can be omitted from this map.
  additional_apex_txt_records = {
    "molyneux.io" = ["MS=ms70605256"]
  }

  app_insights_sampling_percentage = {
    dev = 25
    prd = 75
  }

  action_group_map = {
    critical      = data.terraform_remote_state.platform_monitoring.outputs.monitor_action_groups.critical
    high          = data.terraform_remote_state.platform_monitoring.outputs.monitor_action_groups.high
    moderate      = data.terraform_remote_state.platform_monitoring.outputs.monitor_action_groups.moderate
    low           = data.terraform_remote_state.platform_monitoring.outputs.monitor_action_groups.low
    informational = data.terraform_remote_state.platform_monitoring.outputs.monitor_action_groups.informational
  }

  key_vault_name = "kv-${random_id.environment_id.hex}-${local.location_short}"
}
