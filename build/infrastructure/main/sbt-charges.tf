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

module "sbt_charges" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/service-bus-topic?ref=7.0.0"

  name                = "charges"
  namespace_id        = module.sb_charges.id

  subscriptions       = [
    {
      name                = "command-received"
      max_delivery_count  = 1
    },
    {
      name                = "charge-command-accepted-receiver"
      max_delivery_count  = 1
    },
    {
      name                = "chargeaccepted-sub-dataavailablenotifier"
      max_delivery_count  = 1
    },
    {
      name                = "command-accepted"
      max_delivery_count  = 1
    },
    {
      name                = "command-rejected"
      max_delivery_count  = 1
    },
    {
      name                = "default-charge-links-available-notified"
      max_delivery_count  = 1
    },
    {
      name                = "charge-links-accepted-sub-replier"
      max_delivery_count  = 1
    },
    {
      name                = "charge-links-accepted-sub-event-publisher"
      max_delivery_count  = 1
    },
    {
      name                = "charge-links-accepted-sub-data-available-notifier"
      max_delivery_count  = 1
    },
    {
      name                = "charge-links-accepted-sub-confirmation-notifier"
      max_delivery_count  = 1
    },
    {
      name                = "links-command-received-receiver"
      max_delivery_count  = 1
    },
    {
      name                = "links-command-rejected"
      max_delivery_count  = 1
    },
    {
      name                = "price-command-received"
      max_delivery_count  = 1
    },
    {
      name                = "charge-price-rejected"
      max_delivery_count  = 1
    },
  ]
}