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
output "id" {
  value       = azurerm_storage_account.main.id
  description = "The id of the resource created."
}

output "name" {
  value       = azurerm_storage_account.main.name
  description = "The name of the resource created."
}

output "dependent_on" {
  value       = null_resource.dependency_setter.id
  description = "The dependencies of the resource created."
}

output "primary_connection_string" {
  value       = azurerm_storage_account.main.primary_connection_string
  description = "The connection string associated with the primary location."
  sensitive   = true
}

output "primary_access_key" {
  value       = azurerm_storage_account.main.primary_access_key
  description = "The primary access key for the storage account."
  sensitive   = true
}

output "primary_blob_endpoint" {
  value       = azurerm_storage_account.main.primary_blob_endpoint 
  description = "The endpoint URL for blob storage in the primary location."
}