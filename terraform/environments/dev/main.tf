/**
 * # Development Environment
 * This file contains the configuration for the development environment of the Massage Therapy Booking System.
 */

# Resource Group
resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}-${var.environment}-rg"
  location = var.location

  tags = merge(var.tags, {
    Environment = var.environment
  })
}

# Networking Module
module "networking" {
  source = "../../modules/networking"

  prefix              = var.prefix
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  vnet_address_space  = var.vnet_address_space
  app_subnet_cidr     = var.app_subnet_cidr
  db_subnet_cidr      = var.db_subnet_cidr
  tags                = var.tags
}

# Security Module
module "security" {
  source = "../../modules/security"

  prefix                             = var.prefix
  environment                        = var.environment
  location                           = var.location
  resource_group_name                = azurerm_resource_group.rg.name
  tenant_id                          = var.tenant_id
  app_subnet_id                      = module.networking.app_subnet_id
  allowed_ips                        = var.allowed_ips
  devops_service_principal_object_id = var.devops_service_principal_object_id
  sql_connection_string              = module.database.connection_string
  app_insights_connection_string     = module.monitoring.app_insights_connection_string
  security_alert_emails              = var.security_alert_emails
  tags                               = var.tags

  depends_on = [
    module.networking,
    module.monitoring,
    module.database
  ]
}

# Database Module
module "database" {
  source = "../../modules/database"

  prefix                                 = var.prefix
  environment                            = var.environment
  location                               = var.location
  resource_group_name                    = azurerm_resource_group.rg.name
  administrator_login                    = var.sql_administrator_login
  administrator_login_password           = var.sql_administrator_login_password
  ad_admin_login_username                = var.sql_ad_admin_login_username
  ad_admin_object_id                     = var.sql_ad_admin_object_id
  sql_db_sku                             = var.sql_db_sku
  sql_db_max_size_gb                     = var.sql_db_max_size_gb
  db_subnet_id                           = module.networking.db_subnet_id
  sql_private_dns_zone_id                = module.networking.sql_private_dns_zone_id
  security_storage_account_blob_endpoint = module.security.security_storage_account_primary_blob_endpoint
  security_storage_container_name        = "vulnerability-assessment"
  security_storage_account_access_key    = module.security.security_storage_account_primary_access_key
  security_alert_emails                  = var.security_alert_emails
  tags                                   = var.tags

  depends_on = [
    module.networking
  ]
}

# Monitoring Module
module "monitoring" {
  source = "../../modules/monitoring"

  prefix              = var.prefix
  environment         = var.environment
  location            = var.location
  resource_group_name = azurerm_resource_group.rg.name
  api_app_id          = module.compute.api_app_id
  sql_database_id     = module.database.sql_database_id
  alert_email         = var.alert_email
  tags                = var.tags

  depends_on = [
    module.database,
    module.compute
  ]
}

# Compute Module
module "compute" {
  source = "../../modules/compute"

  prefix                           = var.prefix
  environment                      = var.environment
  location                         = var.location
  resource_group_name              = azurerm_resource_group.rg.name
  app_service_sku                  = var.app_service_sku
  app_subnet_id                    = module.networking.app_subnet_id
  api_cors_allowed_origins         = var.api_cors_allowed_origins
  key_vault_id                     = module.security.key_vault_id
  key_vault_endpoint               = module.security.key_vault_uri
  app_insights_instrumentation_key = module.monitoring.app_insights_instrumentation_key
  app_insights_connection_string   = module.monitoring.app_insights_connection_string
  sql_connection_string            = module.database.connection_string
  azuread_domain                   = var.azuread_domain
  azuread_tenant_id                = var.tenant_id
  azuread_client_id                = module.security.api_application_id
  static_site_sku                  = var.static_site_sku
  static_site_sku_size             = var.static_site_sku_size
  tags                             = var.tags

  depends_on = [
    module.networking,
    module.security,
    module.database,
    module.monitoring
  ]
} 