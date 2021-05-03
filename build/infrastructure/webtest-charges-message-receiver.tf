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
module "webtest_charges_message_receiver" {
  source                          = "./modules/webtest" # Repo geh-terraform-modules doesn't have a webtest module at the time of this writing
  name                            = "webtest-message-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  application_insights_id         = module.appi.id
  http_method                     = "POST"
  url                             = element(split(",", module.azfun_message_receiver.outbound_ip_addresses), 0) # How to get url of function instead of IP address?
}
