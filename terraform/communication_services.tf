resource "azurerm_communication_service" "acs" {
  name                = "acs-${var.workload}-${var.environment}-${var.location}"
  resource_group_name = data.azurerm_resource_group.rg.name
  data_location       = "UK"

  tags = var.tags
}

resource "azurerm_email_communication_service" "email" {
  name                = "ecs-${var.workload}-${var.environment}-${var.location}"
  resource_group_name = data.azurerm_resource_group.rg.name
  data_location       = "UK"

  tags = var.tags
}

# Azure-managed domain (always created - used in dev, available as fallback in prd)
resource "azurerm_email_communication_service_domain" "azure_managed" {
  name              = "AzureManagedDomain"
  email_service_id  = azurerm_email_communication_service.email.id
  domain_management = "AzureManaged"
}

# Custom domains (only in prd - controlled by sending_domains variable)
resource "azurerm_email_communication_service_domain" "custom" {
  for_each = { for d in var.sending_domains : d.name => d }

  name              = each.key
  email_service_id  = azurerm_email_communication_service.email.id
  domain_management = "CustomerManaged"

  lifecycle {
    ignore_changes = [
      # Verification records are managed outside Terraform after initial creation
    ]
  }
}
