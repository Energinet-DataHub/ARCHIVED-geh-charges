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
module "azfun_message_receiver" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.2.0"
  name                                      = "azfun-message-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.main.name
  location                                  = data.azurerm_resource_group.main.location
  storage_account_access_key                = module.azfun_message_receiver_stor.primary_access_key
  app_service_plan_id                       = module.azfun_message_receiver_plan.id
  storage_account_name                      = module.azfun_message_receiver_stor.name
  application_insights_instrumentation_key  = module.appi.instrumentation_key
  tags                                      = data.azurerm_resource_group.main.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE              = true
    WEBSITE_RUN_FROM_PACKAGE                     = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE          = true
    FUNCTIONS_WORKER_RUNTIME                     = "dotnet"
    COMMAND_RECEIVED_SENDER_CONNECTION_STRING    = module.sbtar_command_received_sender.primary_connection_string
    COMMAND_RECEIVED_TOPIC_NAME                  = module.sbt_command_received.name
  }
  dependencies                              = [
    module.appi.dependent_on,
    module.azfun_message_receiver_plan.dependent_on,
    module.azfun_message_receiver_stor.dependent_on,
    module.sbtar_command_received_sender.dependent_on,
  ]
}

module "azfun_message_receiver_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.2.0"
  name                = "asp-message-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Free"
    size  = "F1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_message_receiver_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.2.0"
  name                      = "stormsgrcvr${random_string.message_receiver.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "message_receiver" {
  length  = 6
  special = false
  upper   = false
}
