terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.0"
    }
  }

  required_version = ">= 1.1.0"

  # Remote backend configuration (commented out until storage account is created)
  # backend "azurerm" {
  #   resource_group_name  = "tfstate"
  #   storage_account_name = "massagetherapytfstate"
  #   container_name       = "tfstate"
  #   key                  = "terraform.tfstate"
  # }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }

    resource_group {
      prevent_deletion_if_contains_resources = true
    }

    application_insights {
      disable_generated_rule = false
    }
  }
}

provider "azuread" {
  # Configuration options
} 