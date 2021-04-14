module "sbn_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-namespace?ref=1.0.0"
  name                = "sbn-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = "basic"
  tags                = data.azurerm_resource_group.main.tags
}

module "sbt_local_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.0.0"
  name                = "sbt-local-events"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "sbtar_local_events_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.0.0"
  name                      = "sbtar-local-events-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_local_events.name
}

module "sbtar_local_events_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.0.0"
  name                      = "sbtar-local-events-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_local_events.name
}

resource "azurerm_servicebus_subscription" "sbs-charge-all-events" {
  name                = "sbs-charge-all-events"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  max_delivery_count  = 1
}

resource "azurerm_servicebus_subscription" "sbs-charge-transaction-received-subscription" {
  name                = "sbs-charge-transaction-received-subscription"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  max_delivery_count  = 1
}

resource "azurerm_servicebus_subscription_rule" "sbs-charge-transaction-received-filter" {
  name                = "sbsr-charge-transaction-received-filter"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  subscription_name   = azurerm_servicebus_subscription.sbs-charge-transaction-received-subscription.name
  filter_type         = "SqlFilter"
  sql_filter          = "sys.label = 'FeeCreate' OR sys.label = 'TariffCreate'"
}

resource "azurerm_servicebus_subscription" "sbs-charge-input-validated-received-subscription" {
  name                = "sbs-charge-input-validated-received-subscription"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  max_delivery_count  = 1
}

resource "azurerm_servicebus_subscription_rule" "sbs-charge-input-validated-received-filter" {
  name                = "sbsr-charge-input-validated-received-filter"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  subscription_name   = azurerm_servicebus_subscription.sbs-charge-input-validated-received-subscription.name
  filter_type         = "SqlFilter"
  sql_filter          = "sys.label = 'FeeCreateInputValidationSucceded' OR sys.label = 'TariffCreateInputValidationSucceded'"
}

resource "azurerm_servicebus_subscription" "sbs-business-validated-received-subscription" {
  name                = "sbs-business-validated-received-subscription"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  max_delivery_count  = 1
}

resource "azurerm_servicebus_subscription_rule" "sbs-business-validated-received-filter" {
  name                = "sbsr-business-validated-received-filter"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_local_events.name
  subscription_name   = azurerm_servicebus_subscription.sbs-business-validated-received-subscription.name
  filter_type         = "SqlFilter"
  sql_filter          = "sys.label = 'FeeCreateBusinessValidationSucceded' OR sys.label = 'TariffCreateBusinessValidationSucceded'"
}
