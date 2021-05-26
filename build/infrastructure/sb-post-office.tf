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
Infrastructure for a representation of the Post Office.
This is used to be able to fully explore and integration test the charges domain
without relying on the external dependency to Post Office.

In order to make it lightweight we implement it as an additional topic
on the existing Service Bus Namespace.
=================================================================================
*/

module "sbt_post_office" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic?ref=1.2.0"
  name                = "sbt-post-office"
  namespace_name      = module.sbn_charges.name
  resource_group_name = data.azurerm_resource_group.main.name
}

module "kv_post_office_topic_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "POST-OFFICE-TOPIC-NAME"
  value         = "sbt-post-office"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sbtar_post_office_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-post-office-listener"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  topic_name                = module.sbt_post_office.name
}

module "kv_post_office_listener_connection_string" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name          = "POST-OFFICE-LISTENER-CONNECTION-STRING"
  value         = trimsuffix(module.sbtar_post_office_listener.primary_connection_string, ";EntityPath=${module.sbt_post_office.name}")
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on, module.sbtar_post_office_listener.dependent_on]
}

module "sbtar_post_office_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-topic-auth-rule?ref=1.2.0"
  name                      = "sbtar-post-office-sender"
  namespace_name            = module.sbn_charges.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  topic_name                = module.sbt_post_office.name
}

resource "azurerm_servicebus_subscription" "sbs_post_office" {
  name                = "sbs-post-office"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_post_office.name
  max_delivery_count  = 1
}

module "kv_sbs_post_office" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "POST-OFFICE-SUBSCRIPTION-NAME"
  value         = "sbs-post-office"
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}
