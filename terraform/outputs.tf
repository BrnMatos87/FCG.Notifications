output "function_app_name" {
  description = "Nome da Azure Function App."
  value       = azurerm_function_app_flex_consumption.notifications.name
}

output "function_app_hostname" {
  description = "Hostname público da Azure Function App."
  value       = azurerm_function_app_flex_consumption.notifications.default_hostname
}

output "function_app_base_url" {
  description = "URL base HTTPS da Azure Function App."
  value       = "https://${azurerm_function_app_flex_consumption.notifications.default_hostname}"
}

output "user_created_url" {
  description = "URL do HTTP Trigger de criação de usuário."
  value       = "https://${azurerm_function_app_flex_consumption.notifications.default_hostname}/api/notifications/user-created"
}

output "payment_processed_url" {
  description = "URL do HTTP Trigger de pagamento processado."
  value       = "https://${azurerm_function_app_flex_consumption.notifications.default_hostname}/api/notifications/payment-processed"
}

output "resource_group_name" {
  description = "Nome do Resource Group provisionado."
  value       = azurerm_resource_group.notifications.name
}

output "location" {
  description = "Região Azure dos recursos."
  value       = azurerm_resource_group.notifications.location
}
