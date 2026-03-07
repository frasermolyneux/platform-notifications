resource "azurerm_servicebus_queue" "email_send" {
  name         = "email_send_queue"
  namespace_id = azurerm_servicebus_namespace.sb.id

  max_delivery_count  = 5
  lock_duration       = "PT5M"
  default_message_ttl = "P1D"

  dead_lettering_on_message_expiration = true
}
