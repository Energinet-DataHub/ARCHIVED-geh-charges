# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http:#www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Use this script to execute the scaffolding of the query stack directly from your solution explorer.
#
# Instructions:
#   1. Install necessary tools
#          dotnet tool install --global dotnet-ef
#          dotnet tool install --global dotnet-format
#   2. Execute this script
#   3. Remove old scaffolded classes if no longer exist 
# =======================================================================================================

# Command arguments
$connectionString = "Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"
$context = "QueryDbContext"
$outputDir = "Model"

# Define all tables that should be included in scaffolding.
$tables = "-t Charge -t ChargeLink -t ChargePeriod -t ChargePoint -t DefaultChargeLink -t MarketParticipant -t MeteringPoint -t GridAreaLink"

# Update database model
Invoke-Expression "dotnet build ..\GreenEnergyHub.Charges.ApplyDBMigrationsApp\GreenEnergyHub.Charges.ApplyDBMigrationsApp.csproj"
Invoke-Expression "dotnet run --project ..\\GreenEnergyHub.Charges.ApplyDBMigrationsApp -- ""Server=(localdb)\mssqllocaldb;Database=ChargesDatabase;Trusted_Connection=True;"" includeSeedData"

# Update scaffolded model
# --data-annotations: In order to add keys annotation required by OData
Invoke-Expression "dotnet ef dbcontext scaffold ""$connectionString"" Microsoft.EntityFrameworkCore.SqlServer --output-dir $outputDir --context $context $tables --force --no-onconfiguring --data-annotations"

# Apply .editorconfig styles and fix .editorconfig analyzer violations - e.g. add license header and add empty line between props
Invoke-Expression "dotnet format -s -a --include .\Model\"
