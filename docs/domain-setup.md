# Domain Setup

Guide for adding, removing, or understanding the DNS configuration for ACS sending domains.

## How DNS records are managed

DNS **zones** are created by [platform-connectivity](https://github.com/frasermolyneux/platform-connectivity), but the ACS verification **records** are managed by this repository. This split exists because:

1. **Verification values are computed** — the domain-proof TXT, SPF, DKIM, and DKIM2 values are outputs of `azurerm_email_communication_service_domain`. They only exist after the ACS domain resource is created, which happens in this Terraform apply.
2. **No circular dependencies** — platform-connectivity does not reference this repository's state. Adding a reverse dependency would create a cycle, since this repository already consumes platform-connectivity's remote state for DNS zone information.
3. **Single-apply workflow** — keeping the ACS domain and its DNS records in the same Terraform means a single `terraform apply` creates the domain and provisions all verification records, with no manual copy-paste step.

### Azure DNS apex constraint

Azure DNS treats all TXT records at the same name as **one record set**. The `azurerm_dns_txt_record` resource manages that entire set — you cannot split it across two Terraform configurations. Both the ACS domain-proof and SPF values resolve to the zone apex (`@`), so they must share a single resource (`azurerm_dns_txt_record.acs_apex_txt`).

### Cloudflare difference

Cloudflare's API creates individual DNS records, so there is no record-set conflict. The domain-proof and SPF records remain as separate `cloudflare_dns_record` resources.

## Adding a new sending domain

### 1. Add the domain to `sending_domains`

In `terraform/tfvars/prd.tfvars`:

```hcl
sending_domains = [
  # ... existing domains ...
  { name = "example.com", dns_provider = "azure" },   # or "cloudflare"
]
```

### 2. Check for existing apex TXT records (Azure DNS only)

If the DNS zone already has TXT records at the apex (`@`) — for example a Microsoft 365 `MS=` verification token or an existing SPF record — those values must be added to `local.additional_apex_txt_records` in `terraform/locals.tf`. Without this, the import will **remove** those existing values.

```hcl
additional_apex_txt_records = {
  "molyneux.io"  = ["MS=ms70605256"]
  "example.com"  = ["MS=ms12345678", "google-site-verification=abc123"]  # new domain
}
```

Domains that have no pre-existing apex TXT values can be omitted from the map.

### 3. Remove conflicting records from platform-connectivity

If the DNS zone's JSON file in `platform-connectivity/terraform/zones/` has a `txt_records` entry for the apex (`@`), empty it to avoid two Terraform states managing the same record set:

```json
"txt_records": []
```

**Deploy platform-connectivity first** so that it releases the record from its state before this repository imports it.

### 4. Apply

When this repository is applied, the `import` block in `dns_records.tf` automatically adopts the existing apex TXT record set into state. No manual `terraform import` is needed.

### 5. Grant consumer access

Add an entry to `api_consumers` in the environment tfvars so the calling workload receives the `{domain}.email.sender` app role:

```hcl
api_consumers = [
  {
    workload_name = "my-service"
    display_name  = "id-my-service-prd"
    roles         = ["example.com.email.sender"]
  }
]
```

## Removing a sending domain

1. Remove the domain from `sending_domains` in the environment tfvars.
2. Remove any `additional_apex_txt_records` entry for the domain.
3. Remove any `api_consumers` role references to the domain.
4. Apply — Terraform will destroy the ACS domain, DNS records, and app roles.
5. If platform-connectivity should resume managing the apex TXT records for the zone, re-add entries to the zone JSON file.
