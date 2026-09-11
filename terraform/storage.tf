resource "azurerm_storage_account" "notifications" {
  name                            = var.storage_account_name
  resource_group_name             = azurerm_resource_group.notifications.name
  location                        = azurerm_resource_group.notifications.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "StorageV2"
  min_tls_version                 = "TLS1_2"
  https_traffic_only_enabled      = true
  allow_nested_items_to_be_public = false
  tags                            = local.common_tags
}

resource "azurerm_storage_container" "deployment" {
  name                  = var.deployment_container_name
  storage_account_id    = azurerm_storage_account.notifications.id
  container_access_type = "private"
}
