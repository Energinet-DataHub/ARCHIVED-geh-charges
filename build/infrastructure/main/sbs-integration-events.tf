module "sbs_charges_charge_links_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "metering-point-created-sub"
  project_name        = var.domain_name_short
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "MessageType" = "MeteringPointCreated"
    }
  }
}

module "sbs_charges_charge_links_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "market-participant-changed"
  project_name        = var.domain_name_short
  topic_id            = data.azurerm_key_vault_secret.sbt_domainrelay_integrationevent_received_id.value
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "MessageType" = "MarketParticipantChanged"
    }
  }
}