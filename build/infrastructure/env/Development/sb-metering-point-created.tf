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

/*
=================================================================================
Infrastructure for a representation of the queues of externally published integration events.
This is used to be able to fully explore and integration test the charges domain
without relying on the external dependencies to other domains.

In order to make it lightweight we implement as additional topics
on the existing Service Bus Namespace.
=================================================================================
*/

module "sbt_metering_point_created" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.3.0"
  name                = local.METERING_POINT_CREATED_TOPIC_NAME
  namespace_name      = module.sbn_external_integration_events.name
  resource_group_name = data.azurerm_resource_group.main.name
  dependencies        = [module.sbn_external_integration_events]
}

module "sbtar_metering_point_created_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-metering-point-created-listener"
  namespace_name            = module.sbn_external_integration_events.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [module.sbn_external_integration_events]
  topic_name                = module.sbt_metering_point_created.name
}

# Add the connection string to the "shared" keyvault of the development environment so we can access it the same way in all resouces
module "kv_metering_point_created_listener_connection_string" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                = "metering-point-created-listener-connection-string"
  value               = trimsuffix(module.sbtar_metering_point_created_listener.primary_connection_string, ";EntityPath=${module.sbt_metering_point_created.name}")
  key_vault_id        = module.kv_shared_stub.id
  tags                = data.azurerm_resource_group.main.tags
  dependencies        = [module.kv_shared_stub.dependent_on, module.sbtar_metering_point_created_listener.dependent_on]
}

module "sbtar_metering_point_created_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.3.0"
  name                      = "sbtar-metering-point-created-sender"
  namespace_name            = module.sbn_external_integration_events.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [module.sbn_external_integration_events]
  topic_name                = module.sbt_metering_point_created.name
}

# Add the connection string to the "shared" keyvault of the development environment so we can access it the same way in all resouces
module "kv_metering_point_created_sender_connection_string" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                = "metering-point-created-sender-connection-string"
  value               = trimsuffix(module.sbtar_metering_point_created_sender.primary_connection_string, ";EntityPath=${module.sbt_metering_point_created.name}")
  key_vault_id        = module.kv_shared_stub.id
  tags                = data.azurerm_resource_group.main.tags
  dependencies        = [module.kv_shared_stub.dependent_on, module.sbtar_metering_point_created_sender.dependent_on]
}

resource "azurerm_servicebus_subscription" "sbs_metering_point_created" {
  name                = local.METERING_POINT_CREATED_SUBSCRIPTION_NAME
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_external_integration_events.name
  topic_name          = module.sbt_metering_point_created.name
  max_delivery_count  = 1
}