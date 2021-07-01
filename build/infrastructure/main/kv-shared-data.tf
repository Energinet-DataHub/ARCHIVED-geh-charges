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

# IMPORTANT: This is being overwritten (not just overridden) in Development environment
data "azurerm_key_vault" "kv_sharedresources" {
  name                = var.sharedresources_keyvault_name
  resource_group_name = var.sharedresources_resource_group_name
}

# IMPORTANT: This is being overwritten (not just overridden) in Development environment
data "azurerm_key_vault_secret" "metering_point_created_listener_connection_string" {
  name         = "metering-point-created-listener-connection-string"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}
