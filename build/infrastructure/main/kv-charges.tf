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
module "kv_charges" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault?ref=1.7.0"
  name                            = "kv${var.project}${var.organisation}${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  enabled_for_template_deployment = true
  sku_name                        = "standard"
  
  access_policy = [
    {
      tenant_id               = var.tenant_id
      object_id               = var.spn_object_id
      secret_permissions      = ["set", "get", "list", "delete"]
      certificate_permissions = []
      key_permissions         = []
      storage_permissions     = []
    }
  ]
}
