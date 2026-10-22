locals {
  custom_domain_checks = {
    for check in setproduct(keys(azurerm_email_communication_service_domain.custom), ["SPF", "DKIM", "DKIM2"]) :
    "${check[0]}/${check[1]}" => {
      domain = check[0]
      type   = check[1]
    }
  }
}

# AzureRM creates proof records but does not initiate Azure's verification.
resource "azapi_resource_action" "verify_domain" {
  for_each = azurerm_email_communication_service_domain.custom

  type        = "Microsoft.Communication/emailServices/domains@2023-03-31"
  resource_id = each.value.id
  action      = "initiateVerification"
  body        = { verificationType = "Domain" }
  locks       = [each.value.id]

  depends_on = [
    azurerm_dns_txt_record.acs_apex,
    azurerm_dns_cname_record.acs_dkim,
    azurerm_dns_cname_record.acs_dkim2,
    cloudflare_dns_record.acs_domain_verification,
    cloudflare_dns_record.acs_spf,
    cloudflare_dns_record.acs_dkim,
    cloudflare_dns_record.acs_dkim2,
  ]

  lifecycle {
    replace_triggered_by = [
      azurerm_email_communication_service_domain.custom[each.key],
      azurerm_dns_txt_record.acs_apex,
      azurerm_dns_cname_record.acs_dkim,
      azurerm_dns_cname_record.acs_dkim2,
      cloudflare_dns_record.acs_domain_verification,
      cloudflare_dns_record.acs_spf,
      cloudflare_dns_record.acs_dkim,
      cloudflare_dns_record.acs_dkim2,
    ]
  }

  timeouts {
    create = "20m"
  }
}

resource "azapi_resource_action" "verify_mail_records" {
  for_each = local.custom_domain_checks

  type        = "Microsoft.Communication/emailServices/domains@2023-03-31"
  resource_id = azurerm_email_communication_service_domain.custom[each.value.domain].id
  action      = "initiateVerification"
  body        = { verificationType = each.value.type }
  locks       = [azurerm_email_communication_service_domain.custom[each.value.domain].id]

  depends_on = [azapi_resource_action.verify_domain]

  lifecycle {
    replace_triggered_by = [azapi_resource_action.verify_domain]
  }

  timeouts {
    create = "20m"
  }
}

data "azapi_resource" "verified_domain" {
  for_each = azurerm_email_communication_service_domain.custom

  type                   = "Microsoft.Communication/emailServices/domains@2023-03-31"
  resource_id            = each.value.id
  response_export_values = ["properties.verificationStates"]

  depends_on = [azapi_resource_action.verify_mail_records]

  lifecycle {
    postcondition {
      condition = alltrue([
        for check in ["Domain", "SPF", "DKIM", "DKIM2"] :
        self.output.properties.verificationStates[check].status == "Verified"
      ])
      error_message = "ACS must confirm domain ownership, SPF, DKIM and DKIM2 before linking a sending domain."
    }
  }
}

resource "azurerm_communication_service_email_domain_association" "azure_managed" {
  communication_service_id = azurerm_communication_service.acs.id
  email_service_domain_id  = azurerm_email_communication_service_domain.azure_managed.id
}

resource "azurerm_communication_service_email_domain_association" "custom" {
  for_each = azurerm_email_communication_service_domain.custom

  communication_service_id = azurerm_communication_service.acs.id
  email_service_domain_id  = each.value.id

  depends_on = [data.azapi_resource.verified_domain]
}
