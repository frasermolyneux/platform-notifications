# Look up Cloudflare zones for domains hosted on Cloudflare
data "cloudflare_zone" "domains" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  name = each.key
}

# --- Azure DNS records for ACS domain verification ---

# Domain ownership verification TXT record
resource "azurerm_dns_txt_record" "acs_domain_verification" {
  provider = azurerm.dns

  for_each = { for d in local.azure_dns_domains : d.name => d }

  name                = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].name
  zone_name           = each.key
  resource_group_name = var.dns_resource_group_name
  ttl                 = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].ttl

  record {
    value = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].value
  }

  tags = var.tags
}

# SPF TXT record
resource "azurerm_dns_txt_record" "acs_spf" {
  provider = azurerm.dns

  for_each = { for d in local.azure_dns_domains : d.name => d }

  name                = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].name
  zone_name           = each.key
  resource_group_name = var.dns_resource_group_name
  ttl                 = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].ttl

  record {
    value = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].value
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
resource "cloudflare_record" "acs_domain_verification" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].name
  type    = "TXT"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].domain[0].value
  ttl     = 3600
}

# SPF TXT record
resource "cloudflare_record" "acs_spf" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].name
  type    = "TXT"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].value
  ttl     = 3600
}

# DKIM CNAME record
resource "cloudflare_record" "acs_dkim" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].name
  type    = "CNAME"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim[0].value
  ttl     = 3600
}

# DKIM2 CNAME record
resource "cloudflare_record" "acs_dkim2" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  zone_id = data.cloudflare_zone.domains[each.key].id
  name    = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].name
  type    = "CNAME"
  content = azurerm_email_communication_service_domain.custom[each.key].verification_records[0].dkim2[0].value
  ttl     = 3600
}
