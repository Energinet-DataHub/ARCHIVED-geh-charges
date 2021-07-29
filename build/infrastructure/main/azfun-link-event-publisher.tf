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
module "azfun_link_event_publisher" {
  source                                         = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.7.0"
  name                                           = "azfun-link-event-publisher-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                            = data.azurerm_resource_group.main.name
  location                                       = data.azurerm_resource_group.main.location
  storage_account_access_key                     = module.azfun_link_event_publisher_stor.primary_access_key
  app_service_plan_id                            = module.azfun_link_event_publisher_plan.id
  storage_account_name                           = module.azfun_link_event_publisher_stor.name
  application_insights_instrumentation_key       = module.appi.instrumentation_key
  always_on                                      = true
  tags                                           = data.azurerm_resource_group.main.tags
  app_settings                                   = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE              = true
    WEBSITE_RUN_FROM_PACKAGE                     = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE          = true
    FUNCTIONS_WORKER_RUNTIME                     = "dotnet-isolated"
    LINK_ACCEPTED_LISTENER_CONNECTION_STRING     = module.sbnar_charges_listener.primary_connection_string
    LINK_ACCEPTED_TOPIC_NAME                     = module.sbt_link_command_accepted.name
    LINK_ACCEPTED_SUBSCRIPTION_NAME              = azurerm_servicebus_subscription.sbs_link_command_accepted_event_publisher.name
    INTEGRATIONEVENT_SENDER_CONNECTION_STRING    = data.azurerm_key_vault_secret.integration_events_sender_connection_string.value
    CHARGE_LINK_CREATED_TOPIC_NAME               = local.CHARGE_LINK_CREATED_TOPIC_NAME
    CHARGE_LINK_UPDATED_TOPIC_NAME               = local.CHARGE_LINK_UPDATED_TOPIC_NAME
  } 
  dependencies                                   = [
    module.appi.dependent_on,
    module.azfun_link_event_publisher_plan.dependent_on,
    module.azfun_link_event_publisher_stor.dependent_on,
    module.sbnar_charges_listener.dependent_on,
    module.sbt_link_command_accepted.dependent_on,
  ]
}

module "azfun_link_event_publisher_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.7.0"
  name                = "asp-link-event-publisher-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Basic"
    size  = "B1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_link_event_publisher_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.7.0"
  name                      = "storlevpub${random_string.link_event_publisher.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "link_event_publisher" {
  length  = 6
  special = false
  upper   = false
}

module "ping_webtest_link_event_publisher" {
  source                          = "../modules/ping-webtest" # Repo geh-terraform-modules doesn't have a webtest module at the time of this writing
  name                            = "ping-webtest-link-event-publisher-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  application_insights_id         = module.appi.id
  url                             = "https://${module.azfun_link_event_publisher.default_hostname}/api/HealthStatus"
  dependencies                    = [module.azfun_link_event_publisher.dependent_on]
}

module "mma_ping_webtest_link_event_publisher" {
  source                   = "../modules/availability-alert"
  name                     = "mma-charge-link-event-publisher-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name      = data.azurerm_resource_group.main.name
  application_insight_id   = module.appi.id
  ping_test_name           = module.ping_webtest_link_event_publisher.name
  action_group_id          = module.mag_availabilitity_group.id
  tags                     = data.azurerm_resource_group.main.tags
  dependencies             = [
    module.appi.dependent_on,
    module.ping_webtest_link_event_publisher.dependent_on,
    module.mag_availabilitity_group.dependent_on
  ]
}