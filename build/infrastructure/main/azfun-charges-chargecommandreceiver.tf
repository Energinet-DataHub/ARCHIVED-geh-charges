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
module "azfun_charge_command_receiver" {
  source                                         = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.2.0"
  name                                           = "azfun-charge-command-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                            = data.azurerm_resource_group.main.name
  location                                       = data.azurerm_resource_group.main.location
  storage_account_access_key                     = module.azfun_charge_command_receiver_stor.primary_access_key
  app_service_plan_id                            = module.azfun_charge_command_receiver_plan.id
  storage_account_name                           = module.azfun_charge_command_receiver_stor.name
  application_insights_instrumentation_key       = module.appi.instrumentation_key
  always_on                                      = true
  tags                                           = data.azurerm_resource_group.main.tags
  app_settings                                   = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE              = true
    WEBSITE_RUN_FROM_PACKAGE                     = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE          = true
    FUNCTIONS_WORKER_RUNTIME                     = "dotnet"
    COMMAND_ACCEPTED_SENDER_CONNECTION_STRING    = trimsuffix(module.sbtar_command_accepted_sender.primary_connection_string, ";EntityPath=${module.sbt_command_accepted.name}")
    COMMAND_REJECTED_SENDER_CONNECTION_STRING    = trimsuffix(module.sbtar_command_rejected_sender.primary_connection_string, ";EntityPath=${module.sbt_command_rejected.name}")
    COMMAND_RECEIVED_LISTENER_CONNECTION_STRING  = trimsuffix(module.sbtar_command_received_listener.primary_connection_string, ";EntityPath=${module.sbt_command_received.name}")
    COMMAND_RECEIVED_TOPIC_NAME                  = module.sbt_command_received.name
    COMMAND_ACCEPTED_TOPIC_NAME                  = module.sbt_command_accepted.name
    COMMAND_REJECTED_TOPIC_NAME                  = module.sbt_command_rejected.name
    COMMAND_RECEIVED_SUBSCRIPTION_NAME           = azurerm_servicebus_subscription.sbs_command_received.name
    CHARGE_DB_CONNECTION_STRING                  = local.CHARGE_DB_CONNECTION_STRING
    LOCAL_TIMEZONENAME                           = local.LOCAL_TIMEZONENAME
  } 
  dependencies                                   = [
    module.appi.dependent_on,
    module.azfun_charge_command_receiver_plan.dependent_on,
    module.azfun_charge_command_receiver_stor.dependent_on,
    module.sbtar_command_received_listener.dependent_on,
    module.sbtar_command_accepted_sender.dependent_on,
    module.sbtar_command_rejected_sender.dependent_on,
    module.sbt_command_received.dependent_on,
    module.sbt_command_accepted.dependent_on,
    module.sbt_command_rejected.dependent_on,
  ]
}

module "azfun_charge_command_receiver_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.2.0"
  name                = "asp-charge-command-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_charge_command_receiver_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.2.0"
  name                      = "stormsgrcvr${random_string.charge_command_receiver.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "charge_command_receiver" {
  length  = 6
  special = false
  upper   = false
}

module "ping_webtest_charge_command_receiver" {
  source                          = "./modules/ping-webtest" # Repo geh-terraform-modules doesn't have a webtest module at the time of this writing
  name                            = "ping-webtest-charge-command-receiver-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  application_insights_id         = module.appi.id
  url                             = "https://${module.azfun_charge_command_receiver.default_hostname}/api/HealthStatus"
  dependencies                    = [module.azfun_charge_command_receiver.dependent_on]
}
