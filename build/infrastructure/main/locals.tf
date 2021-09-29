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

locals {
    sqlServerAdminName                            = "gehdbadmin"
    CHARGE_DB_CONNECTION_STRING                   = "Server=tcp:${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_URL.value},1433;Initial Catalog=${module.sqldb_charges.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_ADMIN_NAME.value};Password=${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_ADMIN_PASSWORD.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    LOCAL_TIMEZONENAME                            = "Europe/Copenhagen"
    # All below this line must match the names used in the repo geh-shared-resources
    CHARGE_LINK_CREATED_TOPIC_NAME                = "charge-link-created"
    CHARGE_LINK_UPDATED_TOPIC_NAME                = "charge-link-updated"
    METERING_POINT_CREATED_TOPIC_NAME             = "metering-point-created"
    METERING_POINT_CREATED_SUBSCRIPTION_NAME      = "metering-point-created-sub-charges"
    # The string value is the shared keyvault key name
    INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING = "INTEGRATION-EVENTS-LISTENER-CONNECTION-STRING"
    # The string value is the shared keyvault key name
    INTEGRATION_EVENTS_SENDER_CONNECTION_STRING   = "INTEGRATION-EVENTS-SENDER-CONNECTION-STRING"
}