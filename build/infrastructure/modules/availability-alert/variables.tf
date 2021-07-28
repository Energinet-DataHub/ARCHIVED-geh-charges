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
  description = "(Required) Specifies the name of the storage account. Changing this forces a new resource to be created. This must be unique across the entire Azure service, not just within the resource group."
}

variable resource_group_name {
  type        = string
  description = "(Required) The name of the resource group in which to create the storage account. Changing this forces a new resource to be created."
}

variable application_insight_id {
  type        = string
  description = "(Required) ID of application insight to use for monitoring"
}

variable ping_test_name {
  type        = string
  description = "(Required) Name of the ping test to monitor"
}

variable action_group_id {
  type        = string
  description = "(Required) ID of the action group to trigger when an alert is raised"
}

variable description {
  type        = string
  description = "(Optional) Description shown for the alert rules"
  default     = "Action will be triggered when ping test has failed more than x times within the specified frequency"
}

variable threshold {
  type        = number
  description = "(Optional) The number of times the availability test needs to fail before issuing an alert"
  default     = 3
}

variable frequency {
  type        = string
  description = "(Optional) How often this availability is monitored, represented in ISO 8601 duration format. Possible values are PT1M, PT5M, PT15M, PT30M and PT1H."
  default     = "PT5M"
}

variable window_size {
  type        = string
  description = "(Optional) Time window to consider when monitoring for the threshold. Possible values are PT1M, PT5M, PT15M, PT30M, PT1H, PT6H, PT12H and P1D."
  default     = "PT30M"
}

variable tags {
  type        = any
  description = "(Optional) A mapping of tags to assign to the resource."
  default     = {}
}

variable dependencies {
  type        = list
  description = "A mapping of dependencies which this module depends on."
  default     = []
}