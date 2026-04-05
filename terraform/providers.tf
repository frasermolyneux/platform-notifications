terraform {
  required_version = ">= 1.14.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.67.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.8.0"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 5.18.0"
    }
  }

  backend "azurerm" {}
}

provider "azurerm" {
  subscription_id = var.subscription_id

  features {
    application_insights {
      disable_generated_rule = true
    }

    resource_group {
      prevent_deletion_if_contains_resources = false
    }

    api_management {
      purge_soft_delete_on_destroy = false
    }
  }

  storage_use_azuread = true
}

provider "azurerm" {
  alias           = "dns"
  subscription_id = var.dns_subscription_id

  features {}

  storage_use_azuread = true
}

provider "azuread" {}

provider "cloudflare" {
  api_token = var.cloudflare_api_token
}
