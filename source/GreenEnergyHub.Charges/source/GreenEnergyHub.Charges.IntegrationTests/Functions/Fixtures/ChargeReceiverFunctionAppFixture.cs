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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GreenEnergyHub.FunctionApp.TestCommon;
using GreenEnergyHub.FunctionApp.TestCommon.Azurite;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTests.Functions.Fixtures
{
    public class ChargeReceiverFunctionAppFixture : FunctionAppFixture
    {
        public ChargeReceiverFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
        }

        private AzuriteManager AzuriteManager { get; }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage =true");
        }

        /// <inheritdoc/>
        protected override Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            AzuriteManager.StartAzurite();

            //// TODO: Initialize/start resource managers and create dependent resources (e.g. database, service bus)

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override Task OnDisposeFunctionAppDependenciesAsync()
        {
            AzuriteManager.Dispose();

            //// TODO: Dispose/stop resource managers and delete created dependent resources (e.g. database, service bus)

            return Task.CompletedTask;
        }
    }
}
