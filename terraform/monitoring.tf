resource "azurerm_monitor_metric_alert" "dlq_depth" {
  count = var.environment == "prd" ? 1 : 0

  name                = "${var.workload}-${var.environment} - dead letter queue depth"
  resource_group_name = data.azurerm_resource_group.rg.name
  scopes              = [azurerm_servicebus_namespace.sb.id]
  description         = "Alerts when dead-letter messages accumulate"

  criteria {
    metric_namespace = "Microsoft.ServiceBus/namespaces"
    metric_name      = "DeadletteredMessages"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = 0
  }

  severity    = 1
  frequency   = "PT5M"
  window_size = "PT15M"

  action {
    action_group_id = local.action_group_map.high.id
  }

  tags = var.tags
}
