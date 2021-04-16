// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using DbUp.Engine;
using GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers;

namespace GreenEnergyHub.Charges.ApplyDBMigrationsApp
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var connectionString = ConnectionStringFactory.GetConnectionString(args);
            var isDryRun = args.Contains("dryRun");

            var preDeployResult = PerformUpgrade(connectionString, EnvironmentFilter.GetPreDeployFilter(args), isDryRun);
            if (!preDeployResult.Successful) { return ResultReporter.ReportResult(preDeployResult); }
            ResultReporter.ReportResult(preDeployResult);

            var result = PerformUpgrade(connectionString, EnvironmentFilter.GetFilter(args), isDryRun);
            if (!result.Successful) { return ResultReporter.ReportResult(result); }
            ResultReporter.ReportResult(result);

            var postDeployResult = PerformUpgrade(connectionString, EnvironmentFilter.GetPostDeployFilter(args), isDryRun);
            return ResultReporter.ReportResult(postDeployResult);
        }

        private static DatabaseUpgradeResult PerformUpgrade(string connectionString, Func<string, bool> filter, bool isDryRun)
        {
            // This is the new way of doing it, but for backwards compatibility, the "old way" is still default.
            var upgrader = UpgradeFactory.GetUpgradeEngine(connectionString, filter, isDryRun);

            var result = upgrader.PerformUpgrade();
            return result;
        }
    }
}
