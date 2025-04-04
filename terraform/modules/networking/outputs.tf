output "vnet_id" {
  description = "The ID of the virtual network"
  value       = azurerm_virtual_network.vnet.id
}

output "vnet_name" {
  description = "The name of the virtual network"
  value       = azurerm_virtual_network.vnet.name
}

output "app_subnet_id" {
  description = "The ID of the app subnet"
  value       = azurerm_subnet.app_subnet.id
}

output "app_subnet_name" {
  description = "The name of the app subnet"
  value       = azurerm_subnet.app_subnet.name
}

output "db_subnet_id" {
  description = "The ID of the database subnet"
  value       = azurerm_subnet.db_subnet.id
}

output "db_subnet_name" {
  description = "The name of the database subnet"
  value       = azurerm_subnet.db_subnet.name
}

output "app_nsg_id" {
  description = "The ID of the app network security group"
  value       = azurerm_network_security_group.app_nsg.id
}

output "db_nsg_id" {
  description = "The ID of the database network security group"
  value       = azurerm_network_security_group.db_nsg.id
}

output "sql_private_dns_zone_id" {
  description = "The ID of the SQL private DNS zone"
  value       = azurerm_private_dns_zone.sql_dns_zone.id
}

output "sql_private_dns_zone_name" {
  description = "The name of the SQL private DNS zone"
  value       = azurerm_private_dns_zone.sql_dns_zone.name
} 