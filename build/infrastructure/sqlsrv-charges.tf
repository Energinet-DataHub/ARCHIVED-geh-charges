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
module "sqlsrv_charges" {
  source                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//sql-server?ref=1.2.0"
  name                          = "sqlsrv-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name           = data.azurerm_resource_group.main.name
  location                      = data.azurerm_resource_group.main.location
  administrator_login           = local.sqlServerAdminName
  administrator_login_password  = random_password.sqlsrv_admin_password.result
  tags                          = data.azurerm_resource_group.main.tags
}

module "sqldb_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//sql-database?ref=1.2.0"
  name                = "sqldb-charges"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  tags                = data.azurerm_resource_group.main.tags
  server_name         = module.sqlsrv_charges.name
  dependencies        = [module.sqlsrv_charges.dependent_on]
}

module "sqlsrv_admin_username" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "SQLSERVER--ADMIN--USER"
  value         = local.sqlServerAdminName
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

module "sqlsrv_admin_password" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.2.0"
  name          = "SQLSERVER--ADMIN--PASSWORD"
  value         = random_password.sqlsrv_admin_password.result
  key_vault_id  = module.kv_charges.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies  = [module.kv_charges.dependent_on]
}

resource "random_password" "sqlsrv_admin_password" {
  length = 16
  special = true
  override_special = "_%@"
}

resource "azurerm_sql_firewall_rule" "sqlsrv_fwrule" {
  name                = "sqlsrv-fwrule-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  server_name         = module.sqlsrv_charges.name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "255.255.255.255"  
}
