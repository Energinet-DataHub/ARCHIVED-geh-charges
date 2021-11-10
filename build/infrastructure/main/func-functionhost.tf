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
module "func_functionhost" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.1.0"

  name                                      = "functionhost"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_shared_instrumentation_key.value
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                      = true
    WEBSITE_RUN_FROM_PACKAGE                             = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                  = true
    FUNCTIONS_WORKER_RUNTIME                             = "dotnet-isolated"

    LOCAL_TIMEZONENAME                                   = "Europe/Copenhagen"
    CURRENCY                                             = "DKK"
    CHARGE_DB_CONNECTION_STRING                          = local.CHARGE_DB_CONNECTION_STRING
    DOMAINEVENT_SENDER_CONNECTION_STRING                 = module.sb_charges.primary_connection_strings["send"]
    DOMAINEVENT_LISTENER_CONNECTION_STRING               = module.sb_charges.primary_connection_strings["listen"]
    CHARGE_CREATED_TOPIC_NAME                            = data.azurerm_key_vault_secret.sbt_charge_created_name.value
    CHARGE_PRICES_UPDATED_TOPIC_NAME                     = data.azurerm_key_vault_secret.sbt_charge_prices_updated_name.value
    CHARGE_LINK_ACCEPTED_TOPIC_NAME                      = module.sbt_link_command_accepted.name
    CHARGELINKACCEPTED_SUB_REPLIER                       = "chargelinkaccepted-sub-replier"
    CHARGELINKACCEPTED_SUB_EVENTPUBLISHER                = "chargelinkaccepted-sub-eventpublisher"
    CHARGELINKACCEPTED_SUB_DATAAVAILABLENOTIFIER         = "chargelinkaccepted-sub-dataavailablenotifier"
    CHARGE_LINK_CREATED_TOPIC_NAME                       = data.azurerm_key_vault_secret.sbt_charge_link_created_name.value
    CHARGE_LINK_RECEIVED_TOPIC_NAME                      = module.sbt_link_command_received.name
    CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME               = "link-command-received-receiver"
    COMMAND_ACCEPTED_TOPIC_NAME                          = module.sbt_command_accepted.name
    COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME          = "charge-command-accepted-receiver"
    CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER             = "chargeaccepted-sub-dataavailablenotifier"
    COMMAND_ACCEPTED_SUBSCRIPTION_NAME                   = "command-accepted"
    COMMAND_RECEIVED_TOPIC_NAME                          = module.sbt_command_received.name
    COMMAND_RECEIVED_SUBSCRIPTION_NAME                   = "command-received"
    COMMAND_REJECTED_TOPIC_NAME                          = module.sbt_command_rejected.name
    COMMAND_REJECTED_SUBSCRIPTION_NAME                   = "command-rejected"
    CREATE_LINK_REQUEST_QUEUE_NAME                       = data.azurerm_key_vault_secret.sbq_create_link_request_name.value
    CREATE_LINK_REPLY_QUEUE_NAME                         = data.azurerm_key_vault_secret.sbq_create_link_reply_name.value
    CREATE_LINK_MESSAGES_REQUEST_QUEUE_NAME              = data.azurerm_key_vault_secret.sbq_create_link_messages_request_name.value
    CREATE_LINK_MESSAGES_REPLY_QUEUE_NAME                = data.azurerm_key_vault_secret.sbq_create_link_messages_reply_name.value
    CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME        = data.azurerm_key_vault_secret.sbt_consumption_metering_point_created_name.value
    CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME = data.azurerm_key_vault_secret.sbs_consumption_metering_point_created_sub_charges_name.value

    # Shared resources
    INTEGRATIONEVENT_SENDER_CONNECTION_STRING            = data.azurerm_key_vault_secret.sb_domain_relay_send_connection_string.value
    INTEGRATIONEVENT_LISTENER_CONNECTION_STRING          = data.azurerm_key_vault_secret.sb_domain_relay_listen_connection_string.value
    INTEGRATIONEVENT_MANAGER_CONNECTION_STRING           = data.azurerm_key_vault_secret.sb_domain_relay_manage_connection_string.value

    # Message Hub
    POST_OFFICE_TOPIC_NAME                               = module.sbt_post_office.name
    MESSAGEHUB_STORAGE_CONNECTION_STRING                 = data.azurerm_key_vault_secret.messagehub_storage_connection_string.value
    MESSAGEHUB_STORAGE_CONTAINER                         = data.azurerm_key_vault_secret.messagehub_storage_container.value
    MESSAGEHUB_DATAAVAILABLE_QUEUE                       = "dataavailable"
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                       = "charges"
    MESSAGEHUB_BUNDLEREPLY_QUEUE                         = "charges-reply"
	
  	# Hub identification
    HUB_SENDER_ID                                        = "5790001330552"
    HUB_SENDER_ROLE_INT_ENUM_VALUE                       = "7"
  }
  
  tags                                      = azurerm_resource_group.this.tags
}