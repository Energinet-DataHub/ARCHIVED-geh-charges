# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

module "sbt_charges_domain_events" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=v8"
  name                = "domain-events"
  namespace_id        = data.azurerm_key_vault_secret.sb_domain_relay_namespace_id.value
  project_name        = var.domain_name_short
}

module "sbts_charges_charge_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandReceivedEvent"
  }
}

module "sbts_charges_charge_command_accepted_publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-command-accepted-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandAcceptedEvent"
  }
}

module "sbts_charges_charge_accepted_dataavailable" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-accepted-dataavailable"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandAcceptedEvent"
  }
}

module "sbts_charges_charge_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-command-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandAcceptedEvent"
  }
}

module "sbts_charges_charge_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-command-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeInformationCommandRejectedEvent"
  }
}

module "sbts_charges_default_charge_links_dataavailable" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "default-charge-links-dataavailable"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksDataAvailableNotifiedEvent"
  }
}

module "sbts_charges_charge_links_accepted_publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksAcceptedEvent"
  }
}

module "sbts_charges_charge_links_accepted_dataavailable" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-dataavailable"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksAcceptedEvent"
  }
}

module "sbts_charges_charge_links_accepted_confirmation" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-confirmation"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label ="ChargeLinksAcceptedEvent"
  }
}

module "sbts_charges_charge_links_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksReceivedEvent"
  }
}

module "sbts_charges_charge_links_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-command-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargeLinksRejectedEvent"
  }
}

module "sbts_charges_charge_price_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceCommandReceivedEvent"
  }
}

module "sbts_charges_charge_price_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsRejectedEvent"
  }
}

module "sbts_charges_charge_price_command_confirmed" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-confirmed"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsConfirmedEvent"
  }
}

module "sbts_charges_charge_price_command_confirmed_dataavailable" {  
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-confirmed-dataavail"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsConfirmedEvent"
  }
}

module "sbts_charges_charge_price_confirmed_publish" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-confirmed-publish"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    label = "ChargePriceOperationsConfirmedEvent"
  }
}

