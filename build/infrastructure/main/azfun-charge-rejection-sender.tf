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
module "azfun_charge_rejection_sender" {
  source                                         = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.7.0"
  name                                           = "azfun-charge-rejection-sender-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                            = data.azurerm_resource_group.main.name
  location                                       = data.azurerm_resource_group.main.location
  storage_account_access_key                     = module.azfun_charge_rejection_sender_stor.primary_access_key
  app_service_plan_id                            = module.azfun_charge_rejection_sender_plan.id
  storage_account_name                           = module.azfun_charge_rejection_sender_stor.name
  application_insights_instrumentation_key       = module.appi.instrumentation_key
  always_on                                      = true
  tags                                           = data.azurerm_resource_group.main.tags
  app_settings                                   = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE              = true
    WEBSITE_RUN_FROM_PACKAGE                     = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE          = true
    FUNCTIONS_WORKER_RUNTIME                     = "dotnet"

    COMMAND_REJECTED_LISTENER_CONNECTION_STRING  = trimsuffix(module.sbtar_command_rejected_listener.primary_connection_string, ";EntityPath=${module.sbt_command_rejected.name}")
    COMMAND_REJECTED_TOPIC_NAME                  = module.sbt_command_rejected.name
    COMMAND_REJECTED_SUBSCRIPTION_NAME           = azurerm_servicebus_subscription.sbs_command_rejected.name

    POST_OFFICE_SENDER_CONNECTION_STRING         = trimsuffix(module.sbtar_post_office_sender.primary_connection_string, ";EntityPath=${module.sbt_post_office.name}")
    POST_OFFICE_TOPIC_NAME                       = module.sbt_post_office.name

    LOCAL_TIMEZONENAME                           = local.LOCAL_TIMEZONENAME
  } 
}

module "azfun_charge_rejection_sender_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.7.0"
  name                = "asp-charge-rejection-sender-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_charge_rejection_sender_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.7.0"
  name                      = "storcharej${random_string.charge_rejection_sender.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "charge_rejection_sender" {
  length  = 6
  special = false
  upper   = false
}

module "ping_webtest_charge_rejection_sender" {
  source                          = "../modules/ping-webtest" # Repo geh-terraform-modules doesn't have a webtest module at the time of this writing
  name                            = "ping-webtest-charge-rejection-sender-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  application_insights_id         = module.appi.id
  url                             = "https://${module.azfun_charge_rejection_sender.default_hostname}/api/HealthStatus"
  dependencies                    = [module.azfun_charge_rejection_sender.dependent_on]
}

module "mma_ping_webtest_charge_rejection_sender" {
  source                   = "../modules/availability-alert"
  name                     = "mma-charge-rejection-sender-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name      = data.azurerm_resource_group.main.name
  application_insight_id   = module.appi.id
  ping_test_name           = module.ping_webtest_charge_rejection_sender.name
  action_group_id          = module.mag_availabilitity_group.id
  tags                     = data.azurerm_resource_group.main.tags
  dependencies             = [
    module.appi.dependent_on,
	module.ping_webtest_charge_rejection_sender.dependent_on,
	module.mag_availabilitity_group.dependent_on
  ]
}