output "app_insights_id" {
  description = "The ID of the Application Insights instance"
  value       = azurerm_application_insights.app_insights.id
}

output "app_insights_name" {
  description = "The name of the Application Insights instance"
  value       = azurerm_application_insights.app_insights.name
}

output "app_insights_instrumentation_key" {
  description = "The instrumentation key of the Application Insights instance"
  value       = azurerm_application_insights.app_insights.instrumentation_key
  sensitive   = true
}

output "app_insights_connection_string" {
  description = "The connection string of the Application Insights instance"
  value       = azurerm_application_insights.app_insights.connection_string
  sensitive   = true
}

output "app_insights_app_id" {
  description = "The App ID of the Application Insights instance"
  value       = azurerm_application_insights.app_insights.app_id
}

output "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace"
  value       = azurerm_log_analytics_workspace.log_analytics_workspace.id
}

output "log_analytics_workspace_name" {
  description = "The name of the Log Analytics workspace"
  value       = azurerm_log_analytics_workspace.log_analytics_workspace.name
}

output "log_analytics_workspace_primary_shared_key" {
  description = "The primary shared key of the Log Analytics workspace"
  value       = azurerm_log_analytics_workspace.log_analytics_workspace.primary_shared_key
  sensitive   = true
}

output "critical_alerts_action_group_id" {
  description = "The ID of the critical alerts action group"
  value       = azurerm_monitor_action_group.critical_alerts.id
} 