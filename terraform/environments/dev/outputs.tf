output "resource_group_name" {
  description = "The name of the resource group"
  value       = azurerm_resource_group.rg.name
}

output "resource_group_id" {
  description = "The ID of the resource group"
  value       = azurerm_resource_group.rg.id
}

# Networking outputs
output "virtual_network_id" {
  description = "The ID of the virtual network"
  value       = module.networking.vnet_id
}

output "app_subnet_id" {
  description = "The ID of the app subnet"
  value       = module.networking.app_subnet_id
}

output "db_subnet_id" {
  description = "The ID of the database subnet"
  value       = module.networking.db_subnet_id
}

# Database outputs
output "sql_server_name" {
  description = "The name of the SQL server"
  value       = module.database.sql_server_name
}

output "sql_database_name" {
  description = "The name of the SQL database"
  value       = module.database.sql_database_name
}

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the SQL server"
  value       = module.database.sql_server_fqdn
}

# Compute outputs
output "api_app_name" {
  description = "The name of the API App Service"
  value       = module.compute.api_app_name
}

output "api_app_default_hostname" {
  description = "The default hostname of the API App Service"
  value       = module.compute.api_app_default_hostname
}

output "frontend_app_name" {
  description = "The name of the Static Web App for the frontend"
  value       = module.compute.frontend_app_name
}

output "frontend_app_default_hostname" {
  description = "The default hostname of the Static Web App for the frontend"
  value       = module.compute.frontend_app_default_hostname
}

# Security outputs
output "key_vault_name" {
  description = "The name of the Key Vault"
  value       = module.security.key_vault_name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = module.security.key_vault_uri
}

# Monitoring outputs
output "app_insights_name" {
  description = "The name of the Application Insights instance"
  value       = module.monitoring.app_insights_name
}

output "log_analytics_workspace_name" {
  description = "The name of the Log Analytics workspace"
  value       = module.monitoring.log_analytics_workspace_name
} 