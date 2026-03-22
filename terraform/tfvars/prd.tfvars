workload    = "platform-notifications"
environment = "prd"
location    = "uksouth"

subscription_id     = "903b6685-c12a-4703-ac54-7ec1ff15ca43"
dns_subscription_id = "db34f572-8b71-40d6-8f99-f29a27612144"

sending_domains = [
  { name = "xtremeidiots.com", dns_provider = "cloudflare" },
  { name = "molyneux.io", dns_provider = "azure" },
  { name = "molyneux.me", dns_provider = "azure" },
  { name = "molyneux.dev", dns_provider = "azure" },
  { name = "geo-location.net", dns_provider = "azure" },
  { name = "craftpledge.org", dns_provider = "azure" }
]

api_consumers = [
  {
    workload_name = "portal-web"
    display_name  = "id-portal-web-prd"
    roles = [
      "xtremeidiots.com.email.sender"
    ]
  }
]

platform_monitoring_state = {
  resource_group_name  = "rg-tf-platform-monitoring-prd-uksouth-01"
  storage_account_name = "sa74f04c5f984e"
  container_name       = "tfstate"
  key                  = "terraform.tfstate"
  subscription_id      = "7760848c-794d-4a19-8cb2-52f71a21ac2b"
  tenant_id            = "e56a6947-bb9a-4a6e-846a-1f118d1c3a14"
}

platform_connectivity_state = {
  resource_group_name  = "rg-tf-platform-connectivity-prd-uksouth-01"
  storage_account_name = "sa98ad99056d00"
  container_name       = "tfstate"
  key                  = "terraform.tfstate"
  subscription_id      = "7760848c-794d-4a19-8cb2-52f71a21ac2b"
  tenant_id            = "e56a6947-bb9a-4a6e-846a-1f118d1c3a14"
}

tags = {
  Environment = "prd"
  Workload    = "platform-notifications"
  DeployedBy  = "GitHub-Terraform"
  Git         = "https://github.com/frasermolyneux/platform-notifications"
}
