resource "azurerm_log_analytics_workspace" "notifications" {
  name                = local.log_analytics_name
  resource_group_name = azurerm_resource_group.notifications.name
  location            = azurerm_resource_group.notifications.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  daily_quota_gb      = var.log_analytics_daily_quota_gb
  tags                = local.common_tags
}

resource "azurerm_application_insights" "notifications" {
  name                                 = local.application_insights_name
  resource_group_name                  = azurerm_resource_group.notifications.name
  location                             = azurerm_resource_group.notifications.location
  workspace_id                         = azurerm_log_analytics_workspace.notifications.id
  application_type                     = "web"
  daily_data_cap_in_gb                 = var.application_insights_daily_data_cap_in_gb
  daily_data_cap_notifications_enabled = true
  tags                                 = local.common_tags
}
