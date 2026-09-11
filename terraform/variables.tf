variable "subscription_id" {
  description = "ID da assinatura Azure onde os recursos serão provisionados."
  type        = string

  validation {
    condition     = can(regex("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", var.subscription_id))
    error_message = "subscription_id deve ser um UUID válido."
  }
}

variable "resource_group_name" {
  description = "Nome do Resource Group exclusivo da FCG.Notifications."
  type        = string

  validation {
    condition     = length(var.resource_group_name) >= 1 && length(var.resource_group_name) <= 90
    error_message = "resource_group_name deve ter entre 1 e 90 caracteres."
  }
}

variable "location" {
  description = "Região Azure que suporta Functions Flex Consumption."
  type        = string
  default     = "brazilsouth"

  validation {
    condition     = length(trimspace(var.location)) > 0
    error_message = "location não pode ser vazia."
  }
}

variable "function_app_name" {
  description = "Nome globalmente único da Azure Function App."
  type        = string

  validation {
    condition     = can(regex("^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,58}[a-zA-Z0-9])?$", var.function_app_name))
    error_message = "function_app_name deve ter de 1 a 60 caracteres alfanuméricos ou hífens e não pode começar ou terminar com hífen."
  }
}

variable "storage_account_name" {
  description = "Nome globalmente único da Storage Account usada pelo runtime e pelo deployment."
  type        = string

  validation {
    condition     = can(regex("^[a-z0-9]{3,24}$", var.storage_account_name))
    error_message = "storage_account_name deve ter entre 3 e 24 caracteres, usando apenas letras minúsculas e números."
  }
}

variable "deployment_container_name" {
  description = "Nome do container privado que armazena os pacotes OneDeploy."
  type        = string
  default     = "function-releases"
}

variable "instance_memory_in_mb" {
  description = "Memória de cada instância Flex Consumption."
  type        = number
  default     = 512

  validation {
    condition     = contains([512, 2048, 4096], var.instance_memory_in_mb)
    error_message = "instance_memory_in_mb deve ser 512, 2048 ou 4096."
  }
}

variable "maximum_instance_count" {
  description = "Limite máximo de instâncias para escala horizontal da Function App."
  type        = number
  default     = 2

  validation {
    condition     = var.maximum_instance_count >= 1 && var.maximum_instance_count <= 1000
    error_message = "maximum_instance_count deve estar entre 1 e 1000."
  }
}

variable "log_analytics_daily_quota_gb" {
  description = "Limite diário de ingestão do Log Analytics em GB."
  type        = number
  default     = 0.1

  validation {
    condition     = var.log_analytics_daily_quota_gb >= 0.023
    error_message = "log_analytics_daily_quota_gb deve ser de pelo menos 0.023 GB."
  }
}

variable "application_insights_daily_data_cap_in_gb" {
  description = "Limite diário de telemetria do Application Insights em GB."
  type        = number
  default     = 0.1

  validation {
    condition     = var.application_insights_daily_data_cap_in_gb >= 0.023
    error_message = "application_insights_daily_data_cap_in_gb deve ser de pelo menos 0.023 GB."
  }
}

variable "tags" {
  description = "Tags adicionais aplicadas aos recursos Azure."
  type        = map(string)
  default     = {}
}
