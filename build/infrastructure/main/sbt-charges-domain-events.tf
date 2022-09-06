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
  name                = "charges_domain_events"
  namespace_id        = module.sb_charges.id
  project_name        = var.domain_name_short
}

module "sbts_charges_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command_received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandReceivedEvent"
    }
  }
}

module "sbts_charges_charge_command_accepted_receiver" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge_command_accepted_receiver"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_charges_chargeaccepted_sub_dataavailablenotifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "chargeaccepted_sub_dataavailablenotifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_charges_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command_accepted"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandAcceptedEvent"
    }
  }
}

module "sbts_charges_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "command_rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeCommandRejectedEvent"
    }
  }
}

module "sbts_charges_default_charge_links_available_notified" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "default_charge_links_available_notified"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksDataAvailableNotifiedEvent"
    }
  }
}

module "sbts_charges_charge_links_accepted_sub_event_publisher" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge_links_accepted_sub_event_publisher"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_charges_charge_links_accepted_sub_data_available_notifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge_links_accepted_sub_data_available_notifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_charges_charge_links_accepted_sub_confirmation_notifier" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge_links_accepted_sub_confirmation_notifier"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksAcceptedEvent"
    }
  }
}

module "sbts_charges_links_command_received_receiver" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "links_command_received_receiver"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksReceivedEvent"
    }
  }
}

module "sbts_charges_links_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "links_command_rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargeLinksRejectedEvent"
    }
  }
}

module "sbts_charges_price_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "price_command_received"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargePriceCommandReceivedEvent"
    }
  }
}

module "sbts_charges_charge_price_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic-subscription?ref=v8"
  name                = "charge_price_rejected"
  project_name        = var.domain_name_short
  topic_id            = module.sbt_charges_domain_events.id
  max_delivery_count  = 1
  correlation_filter  = {
    properties     = {
      "Subject" = "ChargePriceOperationsRejectedEvent"
    }
  }
}