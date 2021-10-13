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
module "sbq_messagehub_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-queue?ref=2.0.0"
  name                = "sbq-charges"
  namespace_name      = module.sbn_external_integration_events.name
  resource_group_name = data.azurerm_resource_group.main.name
  requires_session    = true
}

module "sbq_messagehub_charges_reply" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-queue?ref=2.0.0"
  name                = "sbq-charges-reply"
  namespace_name      = module.sbn_external_integration_events.name
  resource_group_name = data.azurerm_resource_group.main.name
  requires_session    = true
}

module "sbq_messagehub_charges_dequeue" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//service-bus-queue?ref=2.0.0"
  name                = "sbq-charges-dequeue"
  namespace_name      = module.sbn_external_integration_events.name
  resource_group_name = data.azurerm_resource_group.main.name
}