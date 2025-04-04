/**
 * # Monitoring Module
 * This module creates the monitoring resources for the Massage Therapy Booking System, including Application Insights.
 */

# Application Insights
resource "azurerm_application_insights" "app_insights" {
  name                = "${var.prefix}-${var.environment}-appinsights"
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.log_analytics_workspace.id
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-appinsights"
  })
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "log_analytics_workspace" {
  name                = "${var.prefix}-${var.environment}-workspace"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = var.environment == "prod" ? 90 : 30

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-workspace"
  })
}

# Diagnostic Settings for App Service
resource "azurerm_monitor_diagnostic_setting" "api_app_diagnostics" {
  name                       = "api-app-diagnostics"
  target_resource_id         = var.api_app_id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id

  enabled_log {
    category = "AppServiceHTTPLogs"
  }

  enabled_log {
    category = "AppServiceConsoleLogs"
  }

  enabled_log {
    category = "AppServiceAppLogs"
  }

  enabled_log {
    category = "AppServiceAuditLogs"
  }

  metric {
    category = "AllMetrics"
    enabled  = true
  }
}

# Diagnostic Settings for SQL Database
resource "azurerm_monitor_diagnostic_setting" "sql_db_diagnostics" {
  name                       = "sql-db-diagnostics"
  target_resource_id         = var.sql_database_id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log_analytics_workspace.id

  enabled_log {
    category = "SQLInsights"
  }

  enabled_log {
    category = "QueryStoreRuntimeStatistics"
  }

  enabled_log {
    category = "QueryStoreWaitStatistics"
  }

  enabled_log {
    category = "Errors"
  }

  enabled_log {
    category = "AutomaticTuning"
  }

  metric {
    category = "Basic"
    enabled  = true
  }

  metric {
    category = "InstanceAndAppAdvanced"
    enabled  = true
  }
}

# Alert Rules
resource "azurerm_monitor_action_group" "critical_alerts" {
  name                = "${var.prefix}-${var.environment}-critical-alerts"
  resource_group_name = var.resource_group_name
  short_name          = "Critical"

  email_receiver {
    name                    = "admins"
    email_address           = var.alert_email
    use_common_alert_schema = true
  }
}

resource "azurerm_monitor_metric_alert" "high_cpu_alert" {
  name                = "${var.prefix}-${var.environment}-high-cpu-alert"
  resource_group_name = var.resource_group_name
  scopes              = [var.api_app_id]
  description         = "This alert monitors API App CPU usage"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "CpuPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  action {
    action_group_id = azurerm_monitor_action_group.critical_alerts.id
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-high-cpu-alert"
  })
}

resource "azurerm_monitor_metric_alert" "high_memory_alert" {
  name                = "${var.prefix}-${var.environment}-high-memory-alert"
  resource_group_name = var.resource_group_name
  scopes              = [var.api_app_id]
  description         = "This alert monitors API App memory usage"
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT15M"

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "MemoryPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 80
  }

  action {
    action_group_id = azurerm_monitor_action_group.critical_alerts.id
  }

  tags = merge(var.tags, {
    Name = "${var.prefix}-${var.environment}-high-memory-alert"
  })
} 