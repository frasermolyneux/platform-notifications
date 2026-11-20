# Look up Cloudflare zones for domains hosted on Cloudflare
data "cloudflare_zone" "domains" {
  for_each = { for d in local.cloudflare_domains : d.name => d }

  filter = {
    name = each.key
  }
}

# Release the old Azure DNS records without deleting the fallback zones' data.
removed {
  from = azurerm_dns_txt_record.acs_apex
  lifecycle {
    destroy = false
  }
}

removed {
  from = azurerm_dns_cname_record.acs_dkim
  lifecycle {
    destroy = false
  }
}

removed {
  from = azurerm_dns_cname_record.acs_dkim2
  lifecycle {
    destroy = false
  }
}

locals {
  existing_cloudflare_acs_txt_records = {
    "molyneux.io" = {
      zone   = "5648ffe2dfbc8459485fece70e7059fe"
      domain = "a9055731a9de72588ab743371c36b137"
      spf    = "fe69fb1cc436822a5455a72f62e4bdb9"
    }
    "molyneux.dev" = {
      zone   = "8e36be30976964ea3ac57eb4882c0bd4"
      domain = "a59c57a8fa215a74b4ff66274d3042c0"
      spf    = "5acd54f7e797fd4493d2f7f89de0c126"
    }
    "geo-location.net" = {
      zone   = "f3fd7ec770e796be651a15ae1b33aa74"
      domain = "ff9ac9b6039127e80911cd9af0178c6f"
      spf    = "2631a0310557e146b40cdedfc6fadfd7"
    }
    "craftpledge.org" = {
      zone   = "4d4b5640294da5e5579e0cb311a02bf1"
      domain = "d06d7f596e35e777c489d028b9d06780"
      spf    = "3ae328aa8aca288f58f57601ad8626da"
    }
  }
}

import {
  for_each = { for d in local.cloudflare_domains : d.name => local.existing_cloudflare_acs_txt_records[d.name] if contains(keys(local.existing_cloudflare_acs_txt_records), d.name) }
  to       = cloudflare_dns_record.acs_domain_verification[each.key]
  id       = "${each.value.zone}/${each.value.domain}"
}

import {
  for_each = { for d in local.cloudflare_domains : d.name => local.existing_cloudflare_acs_txt_records[d.name] if contains(keys(local.existing_cloudflare_acs_txt_records), d.name) }
  to       = cloudflare_dns_record.acs_spf[each.key]
  id       = "${each.value.zone}/${each.value.spf}"
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
  content = each.value.spf_record != null ? each.value.spf_record : azurerm_email_communication_service_domain.custom[each.key].verification_records[0].spf[0].value
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
