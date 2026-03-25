# Look up Cloudflare zones for domains hosted on Cloudflare
data "cloudflare_zone" "domains" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  filter = {
    name = each.key
  }
}

# --- Azure DNS records for ACS domain verification ---
#
# DNS zones are created by platform-connectivity, but the ACS verification
# records are managed HERE because:
#
#   1. The verification values (domain proof, SPF, DKIM) are computed outputs
#      of azurerm_email_communication_service_domain — they only exist after
#      the ACS domain resource is created in this Terraform apply.
#   2. platform-connectivity has no remote state reference to this repo and
#      adding one would create a circular dependency.
#   3. Cloudflare's API creates individual records, but Azure DNS manages all
#      TXT records at the same name as a single record set. Both the domain
#      verification and SPF values resolve to the zone apex (@), so they MUST
#      be in one azurerm_dns_txt_record resource.
#
# When adding a new Azure DNS domain to sending_domains:
#   - If the zone already has apex TXT records, add their values to
#     local.additional_apex_txt_records so they are preserved on import.
#   - Remove those values from platform-connectivity to avoid state conflicts.
#   - The import block below will automatically adopt the existing record set.

# Import existing apex TXT record sets into state. Azure DNS zones may already
# have apex TXT records (e.g. from platform-connectivity or manual creation).
# After a successful apply, these import blocks become no-ops.
import {
  for_each = { for d in local.azure_dns_domains : d.name => d }
  to       = azurerm_dns_txt_record.acs_apex_txt[each.key]
  id       = "/subscriptions/${var.dns_subscription_id}/resourceGroups/${var.dns_resource_group_name}/providers/Microsoft.Network/dnsZones/${each.key}/TXT/${each.key}"
}

resource "azurerm_dns_txt_record" "acs_apex_txt" {
  provider = azurerm.dns

  for_each = { for d in local.azure_dns_domains : d.name => d }

  name                = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].name
  zone_name           = each.key
  resource_group_name = var.dns_resource_group_name
  ttl                 = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].ttl

  record {
    value = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].value
  }

  record {
    value = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].value
  }

  # Preserve pre-existing apex TXT values (e.g. Microsoft 365 domain
  # verification) that were migrated from platform-connectivity.
  # See local.additional_apex_txt_records to add entries for new domains.
  dynamic "record" {
    for_each = lookup(local.additional_apex_txt_records, each.key, [])
    content {
      value = record.value
    }
  }

  tags = var.tags
}

# DKIM CNAME record
resource "azurerm_dns_cname_record" "acs_dkim" {
  provider = azurerm.dns

  for_each = { for d in local.azure_dns_domains : d.name => d }

  name                = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].name
  zone_name           = each.key
  resource_group_name = var.dns_resource_group_name
  ttl                 = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].ttl
  record              = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].value

  tags = var.tags
}

# DKIM2 CNAME record
resource "azurerm_dns_cname_record" "acs_dkim2" {
  provider = azurerm.dns

  for_each = { for d in local.azure_dns_domains : d.name => d }

  name                = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].name
  zone_name           = each.key
  resource_group_name = var.dns_resource_group_name
  ttl                 = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].ttl
  record              = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].value

  tags = var.tags
}

# --- Cloudflare DNS records for ACS domain verification ---

# Domain ownership verification TXT record
resource "cloudflare_dns_record" "acs_domain_verification" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].name
  type    = "TXT"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].value
  ttl     = 3600
}

# SPF TXT record
resource "cloudflare_dns_record" "acs_spf" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].name
  type    = "TXT"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].value
  ttl     = 3600
}

# DKIM CNAME record
resource "cloudflare_dns_record" "acs_dkim" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].name
  type    = "CNAME"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].value
  ttl     = 3600
}

# DKIM2 CNAME record
resource "cloudflare_dns_record" "acs_dkim2" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].name
  type    = "CNAME"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].value
  ttl     = 3600
}
