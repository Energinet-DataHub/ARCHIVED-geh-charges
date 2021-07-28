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
variable name {
  type        = string
  description = "(Required) Specifies the name of the Function App. Changing this forces a new resource to be created."
}

variable resource_group_name {
  type        = string
  description = "(Required) The name of the resource group in which to create the Function App."
}

variable location {
  type        = string
  description = "(Required) Specifies the supported Azure location where the resource exists. Changing this forces a new resource to be created. It needs to correlate with location of parent resource (azurerm_application_insights)."
}

variable tags {
  type        = any
  description = "(Optional) A mapping of tags to assign to the resource."
  default     = {}
}

variable application_insights_id {
  type        = string
  description = "(Required) The ID of the Application Insights component on which the WebTest operates. Changing this forces a new resource to be created."
}

variable frequency {
  type        = number
  description = "(Optional) Interval in seconds between test runs for this WebTest. Valid options are 300, 600 and 900. Defaults to 300."
  default     = 300
}

variable timeout {
  type        = number
  description = "(Optional) Seconds until this WebTest will timeout and fail. Default is 60."
  default     = 60
}

variable enabled {
  type        = bool
  description = "(Optional) Is the test actively being monitored."
  default     = true
}

variable retry_enabled {
  type        = bool
  description = "(Optional) Whether to retry the availability test 3 times before failing it"
  default     = true   
}

variable geo_locations {
  type        = list
  description = "(Optional) A list of where to physically run the tests from to give global coverage for accessibility of your application."
  // See https://docs.microsoft.com/en-us/azure/azure-monitor/app/monitor-web-app-availability
  default     = ["emea-au-syd-edge", "latam-br-gru-edge", "us-tx-sn1-azr", "apac-hk-hkn-azr", "emea-nl-ams-azr"]
}

variable url {
  type        = string
  description = "(Required) Url of HTTP endpoint to test."
}

variable dependencies {
  type        = list
  description = "A mapping of dependencies which this module depends on."
  default     = []
}
