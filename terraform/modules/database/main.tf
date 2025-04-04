/**
 * # Database Module
 * This module creates the Azure SQL Database resources for the Massage Therapy Booking System.
 */

# SQL Server
resource "azurerm_mssql_server" "sql_server" {
  name                          = "${var.prefix}-${var.environment}-sqlserver"
  resource_group_name           = var.resource_group_name
  location                      = var.location
  version                       = "12.0"
  administrator_login           = var.administrator_login
  administrator_login_password  = var.administrator_login_password
  minimum_tls_version           = "1.2"
  public_network_access_enabled = false # Private access only

  azuread_administrator {
    login_username = var.ad_admin_login_username
    object_id      = var.ad_admin_object_id
  }

  identity {
    type = "SystemAssigned"
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-sqlserver"
  })
}

# SQL Database
resource "azurerm_mssql_database" "sql_db" {
  name           = "${var.prefix}-${var.environment}-sqldb"
  server_id      = azurerm_mssql_server.sql_server.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  sku_name       = var.sql_db_sku
  max_size_gb    = var.sql_db_max_size_gb
  zone_redundant = var.environment == "prod" ? true : false

  short_term_retention_policy {
    retention_days           = var.environment == "prod" ? 7 : 1
    backup_interval_in_hours = 24
  }

  long_term_retention_policy {
    weekly_retention  = var.environment == "prod" ? "P4W" : null
    monthly_retention = var.environment == "prod" ? "P12M" : null
    yearly_retention  = var.environment == "prod" ? "P5Y" : null
    week_of_year      = var.environment == "prod" ? 1 : null
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-sqldb"
  })

  # Prevent accidental deletion of this database
  lifecycle {
    prevent_destroy = var.environment == "prod" ? true : false
  }
}

# SQL Firewall Rules
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Private Endpoint
resource "azurerm_private_endpoint" "sql_private_endpoint" {
  name                = "${var.prefix}-${var.environment}-sql-pe"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.db_subnet_id

  private_service_connection {
    name                           = "${var.prefix}-${var.environment}-sql-psc"
    private_connection_resource_id = azurerm_mssql_server.sql_server.id
    is_manual_connection           = false
    subresource_names              = ["sqlServer"]
  }

  private_dns_zone_group {
    name                 = "default"
    private_dns_zone_ids = [var.sql_private_dns_zone_id]
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-sql-pe"
  })
}

# Auditing & Security
resource "azurerm_mssql_server_security_alert_policy" "security_alert_policy" {
  resource_group_name  = var.resource_group_name
  server_name          = azurerm_mssql_server.sql_server.name
  state                = "Enabled"
  email_account_admins = true
}

resource "azurerm_mssql_server_vulnerability_assessment" "vulnerability_assessment" {
  server_security_alert_policy_id = azurerm_mssql_server_security_alert_policy.security_alert_policy.id
  storage_container_path          = "${var.security_storage_account_blob_endpoint}${var.security_storage_container_name}/"
  storage_account_access_key      = var.security_storage_account_access_key

  recurring_scans {
    enabled                   = true
    email_subscription_admins = true
    emails                    = var.security_alert_emails
  }
} 