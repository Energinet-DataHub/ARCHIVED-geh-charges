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
module "sbn_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-namespace?ref=1.3.0"
  name                = "sbn-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = "basic"
  tags                = data.azurerm_resource_group.main.tags
}

module "sbt_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.3.0"
  name                = "sbt-command-received"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "sbtar_command_received_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-received-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_received.name
}

module "sbtar_command_received_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-received-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_received.name
}

resource "azurerm_servicebus_subscription" "sbs_command_received" {
  name                = "sbs-command-received"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_received.name
  max_delivery_count  = 1
}

module "sbt_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.3.0"
  name                = "sbt-command-accepted"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "sbtar_command_accepted_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-accepted-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_accepted.name
}

module "sbtar_command_accepted_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-accepted-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_accepted.name
}

resource "azurerm_servicebus_subscription" "sbs_command_accepted" {
  name                = "sbs-command-accepted"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_accepted.name
  max_delivery_count  = 1
}

module "sbt_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.3.0"
  name                = "sbt-command-rejected"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "sbt_link_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.3.0"
  name                = "sbt-link-command-received"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "sbtar_link_command_received_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-link-command-received-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_link_command_received.name
}

resource "azurerm_servicebus_subscription" "sbs-link-command-received" {
  name                = "sbs-link-command-received"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_link_command_received.name
  max_delivery_count  = 1
}

module "sbtar_command_rejected_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-rejected-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_rejected.name
}

module "sbtar_command_rejected_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-command-rejected-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_rejected.name
}

resource "azurerm_servicebus_subscription" "sbs_command_rejected" {
  name                = "sbs-command-rejected"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_rejected.name
  max_delivery_count  = 1
}
