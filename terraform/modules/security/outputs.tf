output "key_vault_id" {
  description = "The ID of the Key Vault"
  value       = azurerm_key_vault.key_vault.id
}

output "key_vault_name" {
  description = "The name of the Key Vault"
  value       = azurerm_key_vault.key_vault.name
}

output "key_vault_uri" {
  description = "The URI of the Key Vault"
  value       = azurerm_key_vault.key_vault.vault_uri
}

output "security_storage_account_id" {
  description = "The ID of the security storage account"
  value       = azurerm_storage_account.security_storage.id
}

output "security_storage_account_name" {
  description = "The name of the security storage account"
  value       = azurerm_storage_account.security_storage.name
}

output "security_storage_account_primary_blob_endpoint" {
  description = "The primary blob endpoint of the security storage account"
  value       = azurerm_storage_account.security_storage.primary_blob_endpoint
}

output "security_storage_account_primary_access_key" {
  description = "The primary access key of the security storage account"
  value       = azurerm_storage_account.security_storage.primary_access_key
  sensitive   = true
}

output "api_application_id" {
  description = "The application ID of the API app registration"
  value       = azuread_application.api_app_registration.application_id
}

output "frontend_application_id" {
  description = "The application ID of the frontend app registration"
  value       = azuread_application.frontend_app_registration.application_id
}

output "api_service_principal_id" {
  description = "The ID of the API service principal"
  value       = azuread_service_principal.api_service_principal.id
}

output "frontend_service_principal_id" {
  description = "The ID of the frontend service principal"
  value       = azuread_service_principal.frontend_service_principal.id
} 