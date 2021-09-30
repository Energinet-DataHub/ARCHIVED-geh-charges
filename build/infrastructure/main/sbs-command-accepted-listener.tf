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

resource "azurerm_servicebus_subscription" "sbs_command_accepted_charge_command_accepted_receiver" {
  name                = "sbs-charge-command-accepted-receiver"
  resource_group_name = data.azurerm_resource_group.main.name
  namespace_name      = module.sbn_charges.name
  topic_name          = module.sbt_command_accepted.name
  max_delivery_count  = 1
}
