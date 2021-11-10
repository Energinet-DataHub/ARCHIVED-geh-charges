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

module "sbt_link_command_accepted" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=5.1.0"

  name                = "link-command-accepted"
  namespace_name      = module.sb_charges.name
  resource_group_name = azurerm_resource_group.this.name
  subscriptions       = [
    {
      name                = "chargelinkaccepted-sub-replier"
      max_delivery_count  = 1
    },
    {
      name                = "chargelinkaccepted-sub-eventpublisher"
      max_delivery_count  = 1
    },
    {
      name                = "chargelinkaccepted-sub-dataavailablenotifier"
      max_delivery_count  = 1
    },
  ]
}
