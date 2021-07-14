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
    sqlServerAdminName                        = "gehdbadmin"
	CHARGE_DB_CONNECTION_STRING               = "Server=${module.sqlsrv_charges.fully_qualified_domain_name};Database=${module.sqldb_charges.name};Uid=${local.sqlServerAdminName};Pwd=${random_password.sqlsrv_admin_password.result};"
    LOCAL_TIMEZONENAME                        = "Europe/Copenhagen"
    # Must match the name used in the repo geh-shared-resources
    METERING_POINT_CREATED_TOPIC_NAME         = "metering-point-created"
    METERING_POINT_CREATED_SUBSCRIPTION_NAME  = "metering-point-created-sub-charges"
}
