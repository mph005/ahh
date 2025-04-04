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

variable "tenant_id" {
  description = "The tenant ID of the Azure AD"
  type        = string
}

variable "app_subnet_id" {
  description = "The ID of the subnet to allow access from"
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

variable "sql_connection_string" {
  description = "The connection string for the SQL database"
  type        = string
  sensitive   = true
}

variable "app_insights_connection_string" {
  description = "The connection string for Application Insights"
  type        = string
}

variable "security_alert_emails" {
  description = "List of email addresses for security alerts"
  type        = list(string)
  default     = []
} 