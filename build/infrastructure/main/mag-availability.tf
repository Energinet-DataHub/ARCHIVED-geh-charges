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

module "mag_availability_group" {
  source              = "../modules/monitor-action-group-email" # Repo geh-terraform-modules doesn't have a monitor-action-group at the time of writting this
  name                = "mag-availability-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name = azurerm_resource_group.this.name
  short_name          = "a-${lower(var.domain_name_short)}-${lower(var.environment_short)}"
  enabled             = true
  email_receiver      = {
    name              = "Availability notification - ${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
    email_address     = var.notification_email
  }
  tags                = azurerm_resource_group.this.tags
}
