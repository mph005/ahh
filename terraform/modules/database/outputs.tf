output "sql_server_id" {
  description = "The ID of the SQL server"
  value       = azurerm_mssql_server.sql_server.id
}

output "sql_server_name" {
  description = "The name of the SQL server"
  value       = azurerm_mssql_server.sql_server.name
}

output "sql_server_fqdn" {
  description = "The fully qualified domain name of the SQL server"
  value       = azurerm_mssql_server.sql_server.fully_qualified_domain_name
}

output "sql_database_id" {
  description = "The ID of the SQL database"
  value       = azurerm_mssql_database.sql_db.id
}

output "sql_database_name" {
  description = "The name of the SQL database"
  value       = azurerm_mssql_database.sql_db.name
}

output "sql_private_endpoint_id" {
  description = "The ID of the SQL private endpoint"
  value       = azurerm_private_endpoint.sql_private_endpoint.id
}

output "sql_private_endpoint_ip" {
  description = "The private IP address of the SQL private endpoint"
  value       = azurerm_private_endpoint.sql_private_endpoint.private_service_connection[0].private_ip_address
}

output "connection_string" {
  description = "The connection string for the SQL database"
  value       = "Server=tcp:${azurerm_mssql_server.sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.sql_db.name};Persist Security Info=False;User ID=${var.administrator_login};Password=${var.administrator_login_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  sensitive   = true
} 