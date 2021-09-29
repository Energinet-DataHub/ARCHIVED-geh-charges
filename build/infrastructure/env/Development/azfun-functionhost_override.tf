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
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE               = true
    WEBSITE_RUN_FROM_PACKAGE                      = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE           = true
    FUNCTIONS_WORKER_RUNTIME                      = "dotnet-isolated"
    LOCAL_TIMEZONENAME                            = local.LOCAL_TIMEZONENAME
    CHARGE_DB_CONNECTION_STRING                   = local.CHARGE_DB_CONNECTION_STRING
    # Following values is replaced
    INTEGRATIONEVENT_SENDER_CONNECTION_STRING     = module.sbnar_integrationevents_sender.primary_connection_string
    INTEGRATIONEVENT_LISTENER_CONNECTION_STRING   = module.sbnar_integrationevents_listener.primary_connection_string
    # End
    DOMAINEVENT_SENDER_CONNECTION_STRING          = module.sbnar_charges_sender.primary_connection_string
    DOMAINEVENT_LISTENER_CONNECTION_STRING        = module.sbnar_charges_listener.primary_connection_string
    CHARGE_LINK_ACCEPTED_TOPIC_NAME               = module.sbt_link_command_accepted.name
    CHARGE_LINK_ACCEPTED_SUBSCRIPTION_NAME        = azurerm_servicebus_subscription.sbs_link_command_accepted_event_publisher.name
    CHARGE_LINK_CREATED_TOPIC_NAME                = local.CHARGE_LINK_CREATED_TOPIC_NAME
    CHARGE_LINK_RECEIVED_TOPIC_NAME               = module.sbt_link_command_received.name
    CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME        = azurerm_servicebus_subscription.sbs_link_command_received_receiver.name
    COMMAND_ACCEPTED_TOPIC_NAME                   = module.sbt_command_accepted.name
    COMMAND_ACCEPTED_SUBSCRIPTION_NAME            = azurerm_servicebus_subscription.sbs_command_accepted.name
    COMMAND_RECEIVED_TOPIC_NAME                   = module.sbt_command_received.name
    COMMAND_RECEIVED_SUBSCRIPTION_NAME            = azurerm_servicebus_subscription.sbs_command_received.name
    COMMAND_REJECTED_TOPIC_NAME                   = module.sbt_command_rejected.name
    COMMAND_REJECTED_SUBSCRIPTION_NAME            = azurerm_servicebus_subscription.sbs_command_rejected.name
    CREATE_LINK_COMMAND_TOPIC_NAME                = module.sbt_create_link_command.name
    CREATE_LINK_COMMAND_SUBSCRIPTION_NAME         = azurerm_servicebus_subscription.sbs_create_link_command_charges.name
    METERING_POINT_CREATED_TOPIC_NAME             = local.METERING_POINT_CREATED_TOPIC_NAME
    METERING_POINT_CREATED_SUBSCRIPTION_NAME      = local.METERING_POINT_CREATED_SUBSCRIPTION_NAME
    POST_OFFICE_TOPIC_NAME                        = module.sbt_post_office.name
  }
}