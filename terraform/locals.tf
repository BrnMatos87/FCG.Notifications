locals {
  service_plan_name         = "${var.function_app_name}-plan"
  log_analytics_name        = "${var.function_app_name}-logs"
  application_insights_name = "${var.function_app_name}-insights"

  common_tags = merge(
    {
      application = "FCG.Notifications"
      managed-by  = "Terraform"
      workload    = "serverless"
    },
    var.tags
  )
}
