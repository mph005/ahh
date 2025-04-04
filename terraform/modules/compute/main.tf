/**
 * # Compute Module
 * This module creates the Azure App Service resources for the Massage Therapy Booking System.
 */

# App Service Plan
resource "azurerm_service_plan" "app_service_plan" {
  name                = "${var.prefix}-${var.environment}-plan"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Windows"
  sku_name            = var.app_service_sku

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-plan"
  })
}

# API App Service
resource "azurerm_windows_web_app" "api_app" {
  name                = "${var.prefix}-${var.environment}-api"
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.app_service_plan.id
  https_only          = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on              = var.environment == "prod" ? true : false
    minimum_tls_version    = "1.2"
    ftps_state             = "Disabled"
    http2_enabled          = true
    vnet_route_all_enabled = true

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "7.0"
    }

    cors {
      allowed_origins     = var.api_cors_allowed_origins
      support_credentials = true
    }
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                = var.environment == "prod" ? "Production" : var.environment == "staging" ? "Staging" : "Development"
    "WEBSITE_RUN_FROM_PACKAGE"              = "1"
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = var.app_insights_instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = var.app_insights_connection_string
    "KeyVault__Endpoint"                    = var.key_vault_endpoint
    "SQLConnection__UseKeyVault"            = "true"
    "SQLConnection__KeyVaultSecretName"     = "ConnectionString"
    "AzureAd__Domain"                       = var.azuread_domain
    "AzureAd__TenantId"                     = var.azuread_tenant_id
    "AzureAd__ClientId"                     = var.azuread_client_id
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = var.sql_connection_string
  }

  virtual_network_subnet_id = var.app_subnet_id

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-api"
  })

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITE_RUN_FROM_PACKAGE"]
    ]
  }
}

# Frontend App (Static Web App)
resource "azurerm_static_site" "frontend_app" {
  name                = "${var.prefix}-${var.environment}-frontend"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = var.static_site_sku
  sku_size            = var.static_site_sku_size

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-frontend"
  })
}

# API App VNet Integration
resource "azurerm_app_service_virtual_network_swift_connection" "vnet_integration" {
  app_service_id = azurerm_windows_web_app.api_app.id
  subnet_id      = var.app_subnet_id
}

# Key Vault Access Policy for API App
resource "azurerm_key_vault_access_policy" "api_app_access_policy" {
  key_vault_id = var.key_vault_id
  tenant_id    = var.azuread_tenant_id
  object_id    = azurerm_windows_web_app.api_app.identity[0].principal_id

  secret_permissions = [
    "Get", "List"
  ]
} 