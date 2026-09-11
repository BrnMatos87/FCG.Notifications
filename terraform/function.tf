resource "azurerm_service_plan" "notifications" {
  name                = local.service_plan_name
  resource_group_name = azurerm_resource_group.notifications.name
  location            = azurerm_resource_group.notifications.location
  os_type             = "Linux"
  sku_name            = "FC1"
  tags                = local.common_tags
}

resource "azurerm_function_app_flex_consumption" "notifications" {
  name                = var.function_app_name
  resource_group_name = azurerm_resource_group.notifications.name
  location            = azurerm_resource_group.notifications.location
  service_plan_id     = azurerm_service_plan.notifications.id

  storage_container_type      = "blobContainer"
  storage_container_endpoint  = "${azurerm_storage_account.notifications.primary_blob_endpoint}${azurerm_storage_container.deployment.name}"
  storage_authentication_type = "StorageAccountConnectionString"
  storage_access_key          = azurerm_storage_account.notifications.primary_access_key

  runtime_name           = "dotnet-isolated"
  runtime_version        = "8.0"
  instance_memory_in_mb  = var.instance_memory_in_mb
  maximum_instance_count = var.maximum_instance_count
  https_only             = true

  site_config {
    application_insights_connection_string = azurerm_application_insights.notifications.connection_string
    application_insights_key               = azurerm_application_insights.notifications.instrumentation_key
    minimum_tls_version                    = "1.2"
  }

  tags = local.common_tags
}
