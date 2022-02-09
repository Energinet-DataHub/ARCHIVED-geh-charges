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

module "snet_internal_private_endpoints" {
  address_prefixes = ["10.143.7.48/27"]
}

module "snet_external_private_endpoints" {
  address_prefixes = ["10.143.7.80/28"]
}

module "vnet_integrations_webapi" {
  address_prefixes = ["10.143.7.96/28"]
}

module "vnet_integrations_functionhost" {
  address_prefixes = ["10.143.7.112/28"]
}