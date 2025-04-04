/**
 * # Security Module
 * This module creates the security resources for the Massage Therapy Booking System, including Key Vault and AAD B2C.
 */

# Key Vault
resource "azurerm_key_vault" "key_vault" {
  name                        = "${var.prefix}-${var.environment}-kv"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  enabled_for_disk_encryption = true
  tenant_id                   = var.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = var.environment == "prod" ? true : false
  enable_rbac_authorization   = false
  sku_name                    = "standard"

  network_acls {
    default_action             = "Deny"
    bypass                     = "AzureServices"
    ip_rules                   = var.allowed_ips
    virtual_network_subnet_ids = [var.app_subnet_id]
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-kv"
  })
}

# Key Vault Secrets
resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "ConnectionString"
  value        = var.sql_connection_string
  key_vault_id = azurerm_key_vault.key_vault.id
  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.devops_access_policy
  ]
}

resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "AppInsightsConnectionString"
  value        = var.app_insights_connection_string
  key_vault_id = azurerm_key_vault.key_vault.id
  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.devops_access_policy
  ]
}

# Key Vault Access Policy for DevOps Service Principal
resource "azurerm_key_vault_access_policy" "devops_access_policy" {
  key_vault_id = azurerm_key_vault.key_vault.id
  tenant_id    = var.tenant_id
  object_id    = var.devops_service_principal_object_id

  secret_permissions = [
    "Get", "List", "Set", "Delete", "Purge", "Recover"
  ]
}

# Storage Account for Security Features
resource "azurerm_storage_account" "security_storage" {
  name                       = "${var.prefix}${var.environment}sec"
  resource_group_name        = var.resource_group_name
  location                   = var.location
  account_tier               = "Standard"
  account_replication_type   = "LRS"
  min_tls_version            = "TLS1_2"
  https_traffic_only_enabled = true

  network_rules {
    default_action             = "Deny"
    ip_rules                   = var.allowed_ips
    virtual_network_subnet_ids = [var.app_subnet_id]
    bypass                     = ["AzureServices"]
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-security-storage"
  })
}

resource "azurerm_storage_container" "vulnerability_assessment" {
  name                  = "vulnerability-assessment"
  storage_account_id    = azurerm_storage_account.security_storage.id
  container_access_type = "private"
}

# Azure AD B2C Tenant (Note: Azure AD B2C creation is outside of Terraform scope)
# Here we're just creating a reference to a pre-existing B2C tenant
data "azuread_client_config" "current" {}

resource "azuread_application" "api_app_registration" {
  display_name     = "${var.prefix}-${var.environment}-api"
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADMyOrg"

  api {
    requested_access_token_version = 2

    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to access the Massage Therapy Booking API on behalf of the signed-in user."
      admin_consent_display_name = "Access the Massage Therapy Booking API"
      enabled                    = true
      id                         = "00000000-0000-0000-0000-000000000001"
      type                       = "User"
      user_consent_description   = "Allow the application to access the Massage Therapy Booking API on your behalf."
      user_consent_display_name  = "Access the Massage Therapy Booking API"
      value                      = "user_impersonation"
    }
  }

  web {
    redirect_uris = ["https://${var.prefix}-${var.environment}-api.azurewebsites.net/.auth/login/aad/callback"]

    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }
}

resource "azuread_application" "frontend_app_registration" {
  display_name     = "${var.prefix}-${var.environment}-frontend"
  owners           = [data.azuread_client_config.current.object_id]
  sign_in_audience = "AzureADMyOrg"

  web {
    redirect_uris = ["https://${var.prefix}-${var.environment}-frontend.azurestaticapps.net",
    "https://${var.prefix}-${var.environment}-frontend.azurestaticapps.net/authentication/login-callback"]
  }

  required_resource_access {
    resource_app_id = azuread_application.api_app_registration.application_id

    resource_access {
      id   = azuread_application.api_app_registration.oauth2_permission_scope_ids["user_impersonation"]
      type = "Scope"
    }
  }
}

resource "azuread_service_principal" "api_service_principal" {
  client_id = azuread_application.api_app_registration.application_id
}

resource "azuread_service_principal" "frontend_service_principal" {
  client_id = azuread_application.frontend_app_registration.application_id
} 