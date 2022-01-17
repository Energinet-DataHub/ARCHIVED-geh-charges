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

data "azurerm_key_vault" "kv_shared_resources" {
  name                = var.shared_resources_keyvault_name
  resource_group_name = var.shared_resources_resource_group_name
}

data "azurerm_key_vault_secret" "sql_data_admin_name" {
  name         = "sql-data-admin-user-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sql_data_admin_password" {
  name         = "sql-data-admin-user-password"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sql_data_url" {
  name         = "sql-data-url"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sql_data_name" {
  name         = "sql-data-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_storage_connection_string" {
  name         = "st-marketres-primary-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "messagehub_storage_container" {
  name         = "st-marketres-postofficereply-container-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_instrumentation_key" {
  name         = "appi-shared-instrumentation-key"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_name" {
  name         = "appi-shared-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "appi_shared_id" {
  name         = "appi-shared-id"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sb_domain_relay_send_connection_string" {
  name         = "sb-domain-relay-send-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sb_domain_relay_listen_connection_string" {
  name         = "sb-domain-relay-listen-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sb_domain_relay_manage_connection_string" {
  name         = "sb-domain-relay-manage-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sql_actor_register_database_name" {
  name         = "sql-actor-register-database-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_create_link_messages_request_name" {
  name         = "sbq-create-link-messages-request-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_create_link_messages_reply_name" {
  name         = "sbq-create-link-messages-reply-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_create_link_request_name" {
  name         = "sbq-create-link-request-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbq_create_link_reply_name" {
  name         = "sbq-create-link-reply-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_charge_created_name" {
  name         = "sbt-charge-created-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_charge_link_created_name" {
  name         = "sbt-charge-link-created-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_charge_prices_updated_name" {
  name         = "sbt-charge-prices-updated-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbt_consumption_metering_point_created_name" {
  name         = "sbt-consumption-metering-point-created-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}

data "azurerm_key_vault_secret" "sbs_consumption_metering_point_created_sub_charges_name" {
  name         = "sbs-consumption-metering-point-created-sub-charges-name"
  key_vault_id = data.azurerm_key_vault.kv_shared_resources.id
}
