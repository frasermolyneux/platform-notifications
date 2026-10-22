# Domain Setup

Guide for adding, removing, or understanding the DNS configuration for ACS sending domains.

## How DNS records are managed

DNS **zones** are created by [platform-connectivity](https://github.com/frasermolyneux/platform-connectivity), but the ACS verification **records** are managed by this repository. This split exists because:

1. **Verification values are computed** — the domain-proof TXT, SPF, DKIM, and DKIM2 values are outputs of `azurerm_email_communication_service_domain`. They only exist after the ACS domain resource is created, which happens in this Terraform apply.
2. **No circular dependencies** — platform-connectivity does not reference this repository's state. Adding a reverse dependency would create a cycle, since this repository already consumes platform-connectivity's remote state for DNS zone information.
3. **Verified activation** — Terraform provisions the records, initiates Azure's asynchronous Domain, SPF, DKIM and DKIM2 checks, then reads their actual states. Only `Verified` results permit linking custom domains to the communication service. Azure-managed domains are also explicitly linked. DNS creation or HTTP `202` acceptance alone is not verification or email delivery.

### Azure DNS apex constraint

Azure DNS treats all TXT records at the same name as **one record set**. The `azurerm_dns_txt_record` resource manages that entire set — you cannot split it across two Terraform configurations. Both the ACS domain-proof and SPF values resolve to the zone apex (`@`), so they must share a single resource (`azurerm_dns_txt_record.acs_apex`). Azure expects the relative record name `@`, not the full domain name; passing `example.com` would publish at `example.com.example.com`.

The corrected resource uses a new address so the existing `molyneux.io/@` set can actually be imported. The five previous, incorrectly named ACS TXT sets are retired; they are not the live apex sets. No MX, DKIM, DMARC or unrelated TXT records are retired.

### Cloudflare difference

Cloudflare's API creates individual DNS records, so there is no record-set conflict. The domain-proof and SPF records remain as separate `cloudflare_dns_record` resources.

However, a domain must publish **exactly one SPF policy**, regardless of DNS provider. This repository owns that policy for ACS sending domains; do not add another SPF record in platform-connectivity.

### Domains with other sending providers

By default, the SPF policy comes from ACS. When a domain also sends through other providers, set its optional `spf_record` in `sending_domains` to the complete combined policy. The override must retain the authorization required by ACS, not just the additional provider.

For example, `xtremeidiots.com` uses `v=spf1 include:spf.protection.outlook.com include:spf.mandrillapp.com -all` to authorize Microsoft 365/ACS and Mandrill, but not Google. Domains without an override continue to use the ACS-generated policy.

If platform-connectivity already publishes a separate Cloudflare SPF record, deploy the combined policy here first, then deploy removal of only the obsolete SPF record there. These are distinct DNS records, so no Terraform state transfer is required. After both deployments, verify that every authoritative nameserver returns exactly one SPF policy. Preserve unrelated verification TXT and DKIM records.

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

If the DNS zone already has non-SPF TXT records at the apex (`@`) — for example a Microsoft 365 `MS=` verification token — those values must be added to `local.additional_apex_txt_records` in `terraform/locals.tf`. Without this, the import will **remove** those existing values. Merge any existing SPF authorizations into the domain's `spf_record` instead of preserving a second SPF policy in `additional_apex_txt_records`.

```hcl
additional_apex_txt_records = {
  "molyneux.io"  = ["MS=ms70605256"]
  "example.com"  = ["MS=ms12345678", "google-site-verification=abc123"]  # new domain
}
```

Domains that have no pre-existing apex TXT values can be omitted from the map.

### 3. Release conflicting state ownership without deleting DNS

If `platform-connectivity` owns the existing apex set, use a reviewed, declarative ownership transfer. Removing its JSON entry alone would **delete live DNS** and must not be used as a release.

First move the specific instance to a separately named resource and deploy that revision with a production plan proving zero record changes. In a second reviewed revision, remove that isolated resource and only the apex JSON entry, using a `removed` block with `lifecycle { destroy = false }`. Preserve every other TXT entry. Deploy the release and verify the live apex set and SPF are unchanged before importing here.

The current handoff isolates `molyneux.io/@` in connectivity's `azurerm_dns_txt_record.notification_apex[0]`. The other four configured Azure zones were checked and have no pre-existing apex TXT set or connectivity apex ownership.

### 4. Apply

The `import` block adopts the existing `molyneux.io` `/TXT/@` set after its ownership release. Other configured Azure apex sets are created, not imported. For any future domain with an existing set, add it to the import selection only after completing the same ownership and value-preservation checks. No local `terraform import`, state removal or targeted apply is used.

Verification uses the Azure management API through the AzAPI provider because AzureRM does not initiate DNS verification. Requests are serialized per domain, awaited through Azure's long-running operation, and followed by a fail-closed read of all four required statuses. An absent or failed verification blocks association and deployment; do not mark it healthy or resend queued email to compensate. Preserve existing SPF overrides, including the combined Microsoft 365/Mandrill policy for `xtremeidiots.com`.

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
