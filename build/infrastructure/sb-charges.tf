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
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-namespace?ref=1.2.0"
  name                = "sbn-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = "basic"
  tags                = data.azurerm_resource_group.main.tags
}

module "sbt_command_received" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.2.0"
  name                = "sbt-command-received"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "kv_command_received_topic_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-RECEIVED-TOPIC-NAME"
  value         = "sbt-command-received"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbtar_command_received_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-received-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_received.name
}

module "kv_command_received_listener_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-RECEIVED-LISTENER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_received_listener.primary_connection_string, ";EntityPath=${module.sbt_command_received.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_received_listener.dependent_on]
}

module "sbtar_command_received_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-received-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_received.name
}

module "kv_command_received_sender_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-RECEIVED-SENDER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_received_sender.primary_connection_string, ";EntityPath=${module.sbt_command_received.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_received_sender.dependent_on]
}

resource "azurerm_servicebus_subscription" "sbs_command_received" {
  name                = "sbs-command-received"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_received.name
  max_delivery_count  = 1
}

module "kv_sbs_command_received" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-RECEIVED-SUBSCRIPTION-NAME"
  value         = "sbs_command_received"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbt_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.2.0"
  name                = "sbt-command-accepted"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "kv_command_accepted_topic_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-ACCEPTED-TOPIC-NAME"
  value         = "sbt-command-accepted"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbtar_command_accepted_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-accepted-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_accepted.name
}

module "kv_command_accepted_listener_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-ACCEPTED-LISTENER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_accepted_listener.primary_connection_string, ";EntityPath=${module.sbt_command_accepted.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_accepted_listener.dependent_on]
}

module "sbtar_command_accepted_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-accepted-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_accepted.name
}

module "kv_command_accepted_sender_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-ACCEPTED-SENDER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_accepted_sender.primary_connection_string, ";EntityPath=${module.sbt_command_accepted.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_accepted_sender.dependent_on]
}

resource "azurerm_servicebus_subscription" "sbs_command_accepted" {
  name                = "sbs-command-accepted"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_accepted.name
  max_delivery_count  = 1
}

module "kv_sbs_command_accepted" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-ACCEPTED-SUBSCRIPTION-NAME"
  value         = "sbs_command_accepted"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbt_command_rejected" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.2.0"
  name                = "sbt-command-rejected"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_charges]
}

module "kv_command_rejected_topic_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-REJECTED-TOPIC-NAME"
  value         = "sbt-command-rejected"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbtar_command_rejected_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-rejected-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_rejected.name
}

module "kv_command_rejected_listener_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-REJECTED-LISTENER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_rejected_listener.primary_connection_string, ";EntityPath=${module.sbt_command_rejected.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_rejected_listener.dependent_on]
}

module "sbtar_command_rejected_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-command-rejected-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_charges]
  topic_name                = module.sbt_command_rejected.name
}

module "kv_command_rejected_sender_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "COMMAND-REJECTED-SENDER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_command_rejected_sender.primary_connection_string, ";EntityPath=${module.sbt_command_rejected.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_command_rejected_sender.dependent_on]
}

resource "azurerm_servicebus_subscription" "sbs_command_rejected" {
  name                = "sbs-command-rejected"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_rejected.name
  max_delivery_count  = 1
}

module "kv_sbs_command_rejected" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "COMMAND-REJECTED-SUBSCRIPTION-NAME"
  value         = "sbs_command_rejected"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}
