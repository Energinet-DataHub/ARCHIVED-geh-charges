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

module "snet_external_private_endpoints" {
  source                                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=6.0.0"
  name                                          = "external-endpoints-subnet"
  project_name                                  = var.domain_name_short
  environment_short                             = var.environment_short
  environment_instance                          = var.environment_instance
  resource_group_name                           = data.azurerm_key_vault_secret.vnet_shared_resource_group_name.value
  virtual_network_name                          = data.azurerm_key_vault_secret.vnet_shared_name.value
  address_prefixes                              = ["10.42.0.32/28"]
  enforce_private_link_endpoint_network_policies  = true
}

module "snet_internal_private_endpoints" {
  source                                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=6.0.0"
  name                                          = "private-endpoints-subnet"
  project_name                                  = var.domain_name_short
  environment_short                             = var.environment_short
  environment_instance                          = var.environment_instance
  resource_group_name                           = data.azurerm_key_vault_secret.vnet_shared_resource_group_name.value
  virtual_network_name                          = data.azurerm_key_vault_secret.vnet_shared_name.value
  address_prefixes                              = ["10.42.0.48/28"]
  enforce_private_link_endpoint_network_policies  = true
  enforce_private_link_service_network_policies = true
}

module "vnet_integrations_webapi" {
  source                                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=6.0.0"
  name                                          = "vnet-integrations-webapi"
  project_name                                  = var.domain_name_short
  environment_short                             = var.environment_short
  environment_instance                          = var.environment_instance
  resource_group_name                           = data.azurerm_key_vault_secret.vnet_shared_resource_group_name.value
  virtual_network_name                          = data.azurerm_key_vault_secret.vnet_shared_name.value
  address_prefixes                              = ["10.42.0.64/28"]
  enforce_private_link_service_network_policies = true

  # Delegate the subnet to "Microsoft.Web/serverFarms"
  delegations =  [{
   name = "delegation"
   service_delegation_name    = "Microsoft.Web/serverFarms"
   service_delegation_actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
  }]
}

module "vnet_integrations_functionhost" {
  source                                        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/subnet?ref=6.0.0"
  name                                          = "vnet-integrations-functionhost"
  project_name                                  = var.domain_name_short
  environment_short                             = var.environment_short
  environment_instance                          = var.environment_instance
  resource_group_name                           = data.azurerm_key_vault_secret.vnet_shared_resource_group_name.value
  virtual_network_name                          = data.azurerm_key_vault_secret.vnet_shared_name.value
  address_prefixes                              = ["10.42.0.80/28"]
  enforce_private_link_service_network_policies = true

  # Delegate the subnet to "Microsoft.Web/serverFarms"
  delegations =  [{
   name = "delegation"
   service_delegation_name    = "Microsoft.Web/serverFarms"
   service_delegation_actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
  }]
}