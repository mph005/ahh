variable "prefix" {
  description = "The prefix used for all resources in this module"
  type        = string
  default     = "massage"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be one of: dev, staging, prod."
  }
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default     = {}
}

variable "app_service_sku" {
  description = "The SKU name for the App Service Plan"
  type        = string
  default     = "P1v2"
}

variable "app_subnet_id" {
  description = "The ID of the subnet for the App Service VNet integration"
  type        = string
}

variable "api_cors_allowed_origins" {
  description = "List of allowed origins for CORS"
  type        = list(string)
  default     = ["https://massage-dev-frontend.azurestaticapps.net", "https://massage-staging-frontend.azurestaticapps.net", "https://massage-prod-frontend.azurestaticapps.net"]
}

variable "app_insights_instrumentation_key" {
  description = "The instrumentation key for Application Insights"
  type        = string
}

variable "app_insights_connection_string" {
  description = "The connection string for Application Insights"
  type        = string
}

variable "key_vault_id" {
  description = "The ID of the Key Vault"
  type        = string
}

variable "key_vault_endpoint" {
  description = "The endpoint URL of the Key Vault"
  type        = string
}

variable "sql_connection_string" {
  description = "The connection string for the SQL database"
  type        = string
  sensitive   = true
}

variable "azuread_domain" {
  description = "The domain of the Azure AD tenant"
  type        = string
}

variable "azuread_tenant_id" {
  description = "The tenant ID of the Azure AD"
  type        = string
}

variable "azuread_client_id" {
  description = "The client ID for the Azure AD application"
  type        = string
}

variable "static_site_sku" {
  description = "The SKU tier for the Static Web App"
  type        = string
  default     = "Standard"
}

variable "static_site_sku_size" {
  description = "The SKU size for the Static Web App"
  type        = string
  default     = "Standard"
} 