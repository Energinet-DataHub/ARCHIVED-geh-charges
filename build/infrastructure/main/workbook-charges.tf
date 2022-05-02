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

data "template_file" "workbook_charges_template" {
  template = file("${path.module}/workbook-charges-template.json")
  vars = {
    workbook_display_name       = "Charges - Stamdataopdatering SLI"
    workbook_id                 = "67d32b14-3f26-4840-a1cc-c47ca4d052de"
    subscription_id             = data.azurerm_subscription.this.subscription_id
    resouce_group_name          = azurerm_resource_group.this.name
    application_insight_name    = data.azurerm_key_vault_secret.appi_shared_name.value
    shared_resouce_group_name   = var.shared_resources_resource_group_name
    workbook_query              = file("${path.module}/workbook-query-query4.json")
    workbook_parameters         = file("${path.module}/workbook-parameters-period.json")
    workbook_components         = [ file("${path.module}/workbook-query-query4.json"), file("${path.module}/workbook-parameters-period.json") ]
  }
}

resource "azurerm_template_deployment" "workbook_charges" {
  name                  = "workbook-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = azurerm_resource_group.this.name
  deployment_mode       = "incremental"
  template_body         = data.template_file.workbook_charges_template.rendered
}
