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
output "sql_server_url" {
  description = "URL for the shared sql server"
  value = module.sqlsrv_charges.fully_qualified_domain_name
  sensitive = true
}

output "charges_sql_database_name" {
  description = "Name of the charges database in the shared sql server"
  value = module.sqldb_charges.name
  sensitive = true
}

output "charges_user_keyvaultname" {
  description = "Name of the secret in the keyvault containing the username for the charges sql database"
  value = module.sqlsrv_admin_username.name
  sensitive = true
}

output "charges_password_keyvaultname" {
  description = "Name of the secret in the keyvault containing the password for the charges sql database"
  value = module.sqlsrv_admin_password.name
  sensitive = true
}

output "kv_charges_name" {
  description = "Name of the keyvault"
  value = module.kv_charges.name
  sensitive = true
}
