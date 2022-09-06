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

module "sbt_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=8.1.0"
  name                = "charges"
  namespace_id        = module.sb_charges.id
  project_name        = var.domain_name_short
}

module "sbts_charges_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandReceivedEvent"
    }
  }
}

module "sbts_charges_charge_command_accepted_receiver" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-command-accepted-receiver"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_charges_chargeaccepted_sub_dataavailablenotifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "chargeaccepted-sub-dataavailablenotifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_charges_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command-accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandRejectedEvent"
    }
  }
}

module "sbts_default_charge_links_available_notified" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "default-charge-links-available-notified"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksDataAvailableNotifiedEvent"
    }
  }
}

module "sbts_charge_links_accepted_sub_event_publisher" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-sub-event-publisher"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_charge_links_accepted_sub_data_available_notifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-sub-data-available-notifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_charge_links_accepted_sub_confirmation_notifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-links-accepted-sub-confirmation-notifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_links_command_received_receiver" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "links-command-received-receiver"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksReceivedEvent"
    }
  }
}

module "sbts_links_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "links-command-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksRejectedEvent"
    }
  }
}

module "sbts_price_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "price-command-received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargePriceCommandReceivedEvent"
    }
  }
}

module "sbts_charge_price_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge-price-rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargePriceOperationsRejectedEvent"
    }
  }
}