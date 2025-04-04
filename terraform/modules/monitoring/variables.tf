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

variable "api_app_id" {
  description = "The ID of the API App"
  type        = string
}

variable "sql_database_id" {
  description = "The ID of the SQL Database"
  type        = string
}

variable "alert_email" {
  description = "The email address to send alerts to"
  type        = string
} 