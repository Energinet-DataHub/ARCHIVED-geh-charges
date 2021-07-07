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

module "azfun_metering_point_created_receiver" {
  app_settings                                   = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                    = true
    WEBSITE_RUN_FROM_PACKAGE                           = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                = true
    FUNCTIONS_WORKER_RUNTIME                           = "dotnet"

    METERING_POINT_CREATED_LISTENER_CONNECTION_STRING  = module.kv_metering_point_created_listener_connection_string.value
    METERING_POINT_CREATED_TOPIC_NAME                  = local.METERING_POINT_CREATED_TOPIC_NAME
    METERING_POINT_CREATED_SUBSCRIPTION_NAME           = local.METERING_POINT_CREATED_SUBSCRIPTION_NAME

    LOCAL_TIMEZONENAME                                 = local.LOCAL_TIMEZONENAME
  } 
}