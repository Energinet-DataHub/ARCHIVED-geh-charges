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
data "azurerm_sql_server" "sqlsrv" {
  name                = data.azurerm_key_vault_secret.sql_data_name.value
  resource_group_name = data.azurerm_resource_group.shared_resources.name
}

module "sqldb_charges" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/sql-database?ref=5.1.0"

  name                  = "data"
  project_name          = var.domain_name_short
  environment_short     = var.environment_short
  environment_instance  = var.environment_instance
  resource_group_name   = data.azurerm_resource_group.shared_resources.name
  location              = data.azurerm_resource_group.shared_resources.location  
  server_name           = data.azurerm_sql_server.sqlsrv.name
  
  tags                  = azurerm_resource_group.this.tags
}