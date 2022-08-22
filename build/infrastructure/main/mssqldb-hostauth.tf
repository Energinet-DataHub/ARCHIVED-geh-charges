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
resource "null_resource" "create_hosts_as_db_readers_and_writers" {
  triggers = {
    always_run = "${timestamp()}"
  }

  provisioner "local-exec" {
    command     = "${path.module}/scripts/create-hosts-as-readers-and-writers.ps1 -sqlServerName \"${data.azurerm_mssql_server.mssqlsrv.name}\" -databaseName \"${module.mssqldb_charges.name}\" -applicationHosts \"${module.func_functionhost.name}, ${module.app_webapi.name}\""
    interpreter = ["pwsh", "-Command"]
  }
  depends_on = [
      module.mssqldb_charges,
      module.func_functionhost,
      module.app_webapi
    ]
}