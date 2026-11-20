# Domain Setup

Guide for adding, removing, or understanding ACS sending-domain DNS configuration.

## DNS ownership

[platform-connectivity](https://github.com/frasermolyneux/platform-connectivity) manages
Cloudflare zones. This repository manages each sending domain's ACS domain-proof and SPF
TXT records and its DKIM and DKIM2 CNAME records. The values come from the ACS domain
resource, so connectivity must not manage the same records.

Cloudflare stores individual TXT records. Unrelated records, including the Microsoft 365
verification TXT for `molyneux.io`, remain with their existing owner. A domain must have
exactly one SPF policy.

The migration to Cloudflare imports existing ACS proof and SPF records on `molyneux.io`,
`molyneux.dev`, `geo-location.net`, and `craftpledge.org`; it creates the missing ACS
records on `molyneux.me` and missing DKIM records on all five domains. The old Azure DNS
records are released from this Terraform state without deletion. They are no longer
authoritative or maintained by this repository.

## Adding a sending domain

1. Ensure the zone exists in Cloudflare and this workload's token has zone read and DNS
   edit permissions, provisioned through platform-workloads.
2. Check for existing ACS proof, SPF, and DKIM records and their Terraform ownership.
   Coordinate any non-destructive ownership release and import existing records rather
   than creating duplicates. Preserve unrelated TXT and DKIM records.
3. Add the domain to `sending_domains` in the environment tfvars:

   ```hcl
   { name = "example.com", dns_provider = "cloudflare" }
   ```

4. If the domain sends through other providers, set `spf_record` to a single combined
   policy that retains ACS authorization. For example, `xtremeidiots.com` authorizes
   Microsoft 365/ACS and Mandrill with
   `v=spf1 include:spf.protection.outlook.com include:spf.mandrillapp.com -all`.
5. Add any required `api_consumers` entries for the `{domain}.email.sender` app role.
6. Deploy through the existing workflow. Azure's domain, SPF, DKIM, and DKIM2 checks
   must all report `Verified` before a custom domain is linked to the communication
   service. Verify the authoritative DNS records and that only one SPF policy exists.

## Removing a sending domain

Remove the domain from `sending_domains` and any `api_consumers` role references, then
deploy. Terraform removes the ACS domain, its managed DNS records, and app role. Coordinate
any subsequent DNS ownership with platform-connectivity.
