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

locals {
    CHARGE_DB_CONNECTION_STRING                         = "Server=${data.azurerm_key_vault_secret.sql_data_url.value};Database=${module.sqldb_charges.name};Uid=${data.azurerm_key_vault_secret.sql_data_admin_name.value};Pwd=${data.azurerm_key_vault_secret.sql_data_admin_password.value};"
    LOCAL_TIMEZONENAME                                  = "Europe/Copenhagen"
    CURRENCY                                            = "DKK"

    ###########################################################################################
    # All below this line must match the names used in the repo geh-shared-resources
    ###########################################################################################

    CHARGE_LINK_CREATED_TOPIC_NAME                      = "charge-link-created"
    CHARGE_CREATED_TOPIC_NAME                           = "charge-created"
    CHARGE_PRICES_UPDATED_TOPIC_NAME                    = "charge-prices-updated"
    CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME       = "consumption-metering-point-created"
    CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME= "consumption-metering-point-created-sub-charges"
    CREATE_LINK_REQUEST_QUEUE_NAME                      = "create-link-request"
    CREATE_LINK_REPLY_QUEUE_NAME                        = "create-link-reply"
    CREATE_LINK_MESSAGES_REQUEST_QUEUE_NAME             = "create-link-messages-request"
    CREATE_LINK_MESSAGES_REPLY_QUEUE_NAME               = "create-link-messages-reply"

    # Message Hub
    MESSAGEHUB_DATAAVAILABLE_QUEUE                      = "dataavailable"
    MESSAGEHUB_BUNDLEREQUEST_QUEUE                      = "charges"
    MESSAGEHUB_BUNDLEREPLY_QUEUE                        = "charges-reply"
}