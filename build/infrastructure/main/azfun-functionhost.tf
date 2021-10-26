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
module "azfun_functionhost" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=2.0.0"
  name                                      = "azfun-functionhost-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name                       = data.azurerm_resource_group.main.name
  location                                  = data.azurerm_resource_group.main.location
  storage_account_access_key                = module.azfun_functionhost_stor.primary_access_key
  app_service_plan_id                       = module.asp_charges.id
  storage_account_name                      = module.azfun_functionhost_stor.name
  application_insights_instrumentation_key  = module.appi.instrumentation_key
  tags                                      = data.azurerm_resource_group.main.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                      = true
    WEBSITE_RUN_FROM_PACKAGE                             = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                  = true
    FUNCTIONS_WORKER_RUNTIME                             = "dotnet-isolated"

    LOCAL_TIMEZONENAME                                   = local.LOCAL_TIMEZONENAME
    CURRENCY                                             = local.CURRENCY
    CHARGE_DB_CONNECTION_STRING                          = local.CHARGE_DB_CONNECTION_STRING
    DOMAINEVENT_SENDER_CONNECTION_STRING                 = module.sbnar_charges_sender.primary_connection_string
    DOMAINEVENT_LISTENER_CONNECTION_STRING               = module.sbnar_charges_listener.primary_connection_string
    CHARGE_CREATED_TOPIC_NAME                            = local.CHARGE_CREATED_TOPIC_NAME
    CHARGE_PRICES_UPDATED_TOPIC_NAME                     = local.CHARGE_PRICES_UPDATED_TOPIC_NAME
    CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER             = module.sbs_chargeaccepted_sub_dataavailablenotifier.name
    CHARGE_LINK_ACCEPTED_TOPIC_NAME                      = module.sbt_link_command_accepted.name
    CHARGELINKACCEPTED_SUB_EVENTPUBLISHER                = module.sbs_chargelinkaccepted_sub_eventpublisher.name
    CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER         = module.sbs_chargelinkaccepted_sub_dataavailablenotifier.name
    CHARGE_LINK_CREATED_TOPIC_NAME                       = local.CHARGE_LINK_CREATED_TOPIC_NAME
    CHARGE_LINK_RECEIVED_TOPIC_NAME                      = module.sbt_link_command_received.name
    CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME               = module.sbs_link_command_received_receiver.name
    COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME          = module.sbs_charge_command_accepted_receiver.name
    COMMAND_ACCEPTED_TOPIC_NAME                          = module.sbt_command_accepted.name
    COMMAND_ACCEPTED_SUBSCRIPTION_NAME                   = module.sbs_command_accepted.name
    COMMAND_RECEIVED_TOPIC_NAME                          = module.sbt_command_received.name
    COMMAND_RECEIVED_SUBSCRIPTION_NAME                   = module.sbs_command_received.name
    COMMAND_REJECTED_TOPIC_NAME                          = module.sbt_command_rejected.name
    COMMAND_REJECTED_SUBSCRIPTION_NAME                   = module.sbs_command_rejected.name
    CREATE_LINK_REQUEST_QUEUE_NAME                       = local.CREATE_LINK_REQUEST_QUEUE_NAME
    CREATE_LINK_REPLY_QUEUE_NAME                         = local.CREATE_LINK_REPLY_QUEUE_NAME
    CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME        = local.CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME
    CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME = local.CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME  

    # Shared resources
    INTEGRATIONEVENT_SENDER_CONNECTION_STRING            = data.azurerm_key_vault_secret.integration_events_sender_connection_string.value
    INTEGRATIONEVENT_LISTENER_CONNECTION_STRING          = data.azurerm_key_vault_secret.integration_events_listener_connection_string.value
    INTEGRATIONEVENT_MANAGER_CONNECTION_STRING           = data.azurerm_key_vault_secret.integration_events_manager_connection_string.value

    # Message Hub
    POST_OFFICE_TOPIC_NAME                               = module.sbt_post_office.name
    MESSAGEHUB_STORAGE_CONNECTION_STRING                 = data.azurerm_key_vault_secret.messagehub_storage_connection_string.value
    MESSAGEHUB_STORAGE_CONTAINER                         = data.azurerm_key_vault_secret.messagehub_storage_container.value
    MESSAGEHUB_DATAAVAILABLE_QUEUE                       = local.MESSAGEHUB_DATAAVAILABLE_QUEUE
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                       = local.MESSAGEHUB_BUNDLEREQUEST_QUEUE
    MESSAGEHUB_BUNDLEREPLY_QUEUE                         = local.MESSAGEHUB_BUNDLEREPLY_QUEUE
	
  	# Hub identification
    HUB_SENDER_ID                                        = "5790001330552"
    HUB_SENDER_ROLE_INT_ENUM_VALUE                       = "7"
  }
  dependencies                                          = [
    module.appi.dependent_on,
    module.asp_charges.dependent_on,
    module.azfun_functionhost_stor.dependent_on,
    module.sbnar_charges_sender.dependent_on,
    module.sbt_link_command_received.dependent_on,
  ]
}

module "azfun_functionhost_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.7.0"
  name                      = "storhosts${random_string.functionhost.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "functionhost" {
  length  = 6
  special = false
  upper   = false
}

module "ping_webtest_functionhost" {
  source                          = "../modules/ping-webtest" # Repo geh-terraform-modules doesn't have a webtest module at the time of this writing
  name                            = "ping-webtest-functionhost-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  tags                            = data.azurerm_resource_group.main.tags
  application_insights_id         = module.appi.id
  url                             = "https://${module.azfun_functionhost.default_hostname}/api/HealthStatus"
  dependencies                    = [module.azfun_functionhost.dependent_on]
}

module "mma_ping_webtest_functionhost" {
  source                   = "../modules/availability-alert"
  name                     = "mma-functionhost-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name      = data.azurerm_resource_group.main.name
  application_insight_id   = module.appi.id
  ping_test_name           = module.ping_webtest_functionhost.name
  action_group_id          = module.mag_availability_group.id
  tags                     = data.azurerm_resource_group.main.tags
  dependencies             = [
    module.appi.dependent_on,
    module.ping_webtest_functionhost.dependent_on,
    module.mag_availability_group.dependent_on
  ]
}