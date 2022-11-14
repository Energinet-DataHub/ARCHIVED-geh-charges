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
using System.Reflection;
using DbUp.Reboot;
using DbUp.Reboot.Engine;

namespace GreenEnergyHub.Charges.ApplyDBMigrationsApp.Helpers
{
    public static class UpgradeFactory
    {
        public static UpgradeEngine GetUpgradeEngine(string connectionString, Func<string, bool> scriptFilter, bool isDryRun = false)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string must have a value");
            }

            EnsureDatabase.For.SqlDatabase(connectionString);

            var builder = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptNameComparer(new ScriptComparer())
                .WithScripts(new CustomScriptProvider(Assembly.GetExecutingAssembly(), scriptFilter))
                .LogToConsole()
                .WithExecutionTimeout(TimeSpan.FromMinutes(2));

            if (isDryRun)
            {
                builder.WithTransactionAlwaysRollback();
            }
            else
            {
                builder.WithTransaction();
            }

            return builder.Build();
        }
    }
}
