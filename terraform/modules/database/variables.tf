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

variable "administrator_login" {
  description = "The administrator login for the SQL server"
  type        = string
  sensitive   = true
}

variable "administrator_login_password" {
  description = "The administrator login password for the SQL server"
  type        = string
  sensitive   = true
}

variable "ad_admin_login_username" {
  description = "The login username of the Azure AD administrator for the SQL server"
  type        = string
}

variable "ad_admin_object_id" {
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

variable "db_subnet_id" {
  description = "The ID of the subnet for the private endpoint"
  type        = string
}

variable "sql_private_dns_zone_id" {
  description = "The ID of the private DNS zone for SQL"
  type        = string
}

variable "security_storage_account_blob_endpoint" {
  description = "The blob endpoint of the storage account for vulnerability assessment"
  type        = string
}

variable "security_storage_container_name" {
  description = "The name of the storage container for vulnerability assessment"
  type        = string
  default     = "vulnerability-assessment"
}

variable "security_storage_account_access_key" {
  description = "The access key of the storage account for vulnerability assessment"
  type        = string
  sensitive   = true
}

variable "security_alert_emails" {
  description = "List of email addresses for security alerts"
  type        = list(string)
  default     = []
} 