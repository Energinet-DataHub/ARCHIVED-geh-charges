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

# Note that using this module will generate an alert for functions with
# availability tests that are not yet deployed or are under deployment.
# The default values for threshold and window_size has been picked to 
# mitigate this effect, by allowing deploying to take place without
# triggering an alert. If this is not the use case you need, make sure
# to set threshold and window_size accordingly in your usage.
resource "null_resource" "dependency_getter" {
  provisioner "local-exec" {
    command = "echo ${length(var.dependencies)}"
  }
}
resource "null_resource" "dependency_setter" {
  depends_on = [azurerm_monitor_metric_alert.main]
}

resource "azurerm_monitor_metric_alert" "main" {
  depends_on               = [null_resource.dependency_getter]
  name                     = var.name
  resource_group_name      = var.resource_group_name 
  scopes                   = [ var.application_insight_id ]
  description              = var.description
  
  criteria {
    metric_namespace       = "Microsoft.Insights/components"
    metric_name            = "availabilityResults/count"
    aggregation            = "Count"
    operator               = "GreaterThan"
    threshold              = var.threshold
    dimension {
      name                 = "availabilityResult/name"
      operator             = "Include"
      values               = [ var.ping_test_name ]
    }
	dimension {
      name                 = "availabilityResult/success"
      operator             = "Include"
      values               = [ "0" ]
    }
  }
  action {
    action_group_id        = var.action_group_id
  }
  frequency                = var.frequency
  window_size              = var.window_size
  tags                     = var.tags
}