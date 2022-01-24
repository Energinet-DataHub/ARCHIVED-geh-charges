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
variable subscription_id {
  type = string
}

variable resource_group_name {
  type = string
}

variable vnet_resource_group_name {
  type        = string
  description = "Resource Group that the VNet is created in."
}

variable environment_short {
  type          = string
  description   = "Enviroment that the infrastructure code is deployed into."
}

variable environment_instance {
  type          = string
  description   = "Enviroment instance that the infrastructure code is deployed into."
}

variable domain_name_short {
  type          = string
  description   = "Name of the project this infrastructure is a part of."
}

variable shared_resources_keyvault_name {
  type          = string
  description   = "Name of the Key Vault, that contains the shared secrets"
}

variable shared_resources_resource_group_name {
  type          = string
  description   = "Name of the Resource Group, that contains the shared resources."
}

variable notification_email {
  type          = string
  description   = "Email address to send notifications to"
}
