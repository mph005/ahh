output "app_service_plan_id" {
  description = "The ID of the App Service Plan"
  value       = azurerm_service_plan.app_service_plan.id
}

output "api_app_id" {
  description = "The ID of the API App Service"
  value       = azurerm_windows_web_app.api_app.id
}

output "api_app_name" {
  description = "The name of the API App Service"
  value       = azurerm_windows_web_app.api_app.name
}

output "api_app_default_hostname" {
  description = "The default hostname of the API App Service"
  value       = azurerm_windows_web_app.api_app.default_hostname
}

output "api_app_principal_id" {
  description = "The Principal ID of the API App Service's managed identity"
  value       = azurerm_windows_web_app.api_app.identity[0].principal_id
}

output "frontend_app_id" {
  description = "The ID of the Static Web App for the frontend"
  value       = azurerm_static_site.frontend_app.id
}

output "frontend_app_name" {
  description = "The name of the Static Web App for the frontend"
  value       = azurerm_static_site.frontend_app.name
}

output "frontend_app_default_hostname" {
  description = "The default hostname of the Static Web App for the frontend"
  value       = azurerm_static_site.frontend_app.default_host_name
}

output "frontend_app_api_key" {
  description = "The API key of the Static Web App for the frontend"
  value       = azurerm_static_site.frontend_app.api_key
  sensitive   = true
} 