variable "prefix" {
  description = "The prefix used for all resources"
  type        = string
  default     = "massage"
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "eastus"
}

variable "tags" {
  description = "A map of tags to add to all resources"
  type        = map(string)
  default = {
    Project     = "MassageTherapyBooking"
    ManagedBy   = "Terraform"
    Environment = "Development"
  }
}

# Networking variables
variable "vnet_address_space" {
  description = "The address space for the virtual network"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "app_subnet_cidr" {
  description = "The CIDR block for the app subnet"
  type        = string
  default     = "10.0.1.0/24"
}

variable "db_subnet_cidr" {
  description = "The CIDR block for the database subnet"
  type        = string
  default     = "10.0.2.0/24"
}

# Security variables
variable "tenant_id" {
  description = "The tenant ID of the Azure AD"
  type        = string
}

variable "allowed_ips" {
  description = "List of IP addresses allowed to access the Key Vault"
  type        = list(string)
  default     = []
}

variable "devops_service_principal_object_id" {
  description = "The object ID of the DevOps service principal"
  type        = string
}

variable "security_alert_emails" {
  description = "List of email addresses for security alerts"
  type        = list(string)
  default     = ["admin@example.com"]
}

# Database variables
variable "sql_administrator_login" {
  description = "The administrator login for the SQL server"
  type        = string
  sensitive   = true
}

variable "sql_administrator_login_password" {
  description = "The administrator login password for the SQL server"
  type        = string
  sensitive   = true
}

variable "sql_ad_admin_login_username" {
  description = "The login username of the Azure AD administrator for the SQL server"
  type        = string
}

variable "sql_ad_admin_object_id" {
  description = "The object ID of the Azure AD administrator for the SQL server"
  type        = string
}

variable "sql_db_sku" {
  description = "The SKU name for the SQL database"
  type        = string
  default     = "S0"
}

variable "sql_db_max_size_gb" {
  description = "The maximum size of the SQL database in gigabytes"
  type        = number
  default     = 4
}

# Compute variables
variable "app_service_sku" {
  description = "The SKU name for the App Service Plan"
  type        = string
  default     = "P1v2"
}

variable "api_cors_allowed_origins" {
  description = "List of allowed origins for CORS"
  type        = list(string)
  default     = ["https://massage-dev-frontend.azurestaticapps.net"]
}

variable "azuread_domain" {
  description = "The domain of the Azure AD tenant"
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

# Monitoring variables
variable "alert_email" {
  description = "The email address to send alerts to"
  type        = string
  default     = "admin@example.com"
} 