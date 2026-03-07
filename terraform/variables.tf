variable "workload" {
  default = "platform-notifications"
}

variable "environment" {
  default = "dev"
}

variable "location" {
  default = "swedencentral"
}

variable "subscription_id" {}

variable "dns_subscription_id" {
  description = "Subscription ID for the platform-connectivity DNS zones."
}

variable "dns_resource_group_name" {
  description = "Resource group name containing the DNS zones."
  default     = "rg-platform-dns-prd-uksouth-01"
}

variable "cloudflare_api_token" {
  description = "Cloudflare API token for managing DNS records on Cloudflare-hosted domains."
  default     = ""
  sensitive   = true
}

variable "sending_domains" {
  description = "Domains to configure for email sending via ACS."
  type = list(object({
    name         = string
    dns_provider = string
  }))
  default = []
}

variable "api_consumers" {
  description = "Consumer workloads that need app role assignments on the notification API."
  type = list(object({
    workload_name = string
    display_name  = string
    roles         = list(string)
  }))
  default = []
}

variable "platform_monitoring_state" {
  description = "Backend config for platform-monitoring remote state."
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
    subscription_id      = string
    tenant_id            = string
  })
}

variable "platform_connectivity_state" {
  description = "Backend config for platform-connectivity remote state."
  type = object({
    resource_group_name  = string
    storage_account_name = string
    container_name       = string
    key                  = string
    subscription_id      = string
    tenant_id            = string
  })
}

variable "tags" {
  default = {}
}
