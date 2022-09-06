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

using System.Collections.Generic;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    /// <summary>
    /// Responsible for extracting secrets necessary for performing system tests of API Management.
    ///
    /// On developer machines we use the 'systemtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access to the Key Vault
    /// so secrets can be extracted.
    /// </summary>
    public class ApiManagementConfiguration : SystemTestConfiguration
    {
        public ApiManagementConfiguration()
        {
            var environment =
                Root.GetValue<string>("ENVIRONMENT_SHORT") +
                Root.GetValue<string>("ENVIRONMENT_INSTANCE");

            var clientNames = new List<string> { Root.GetValue<string>("CLIENT_NAME") };

            AuthorizationConfiguration = new B2CAuthorizationConfiguration(
                usedForSystemTests: true,
                environment,
                clientNames);
        }

        /// <summary>
        /// Configuration for Azure Authorization
        /// </summary>
        public B2CAuthorizationConfiguration AuthorizationConfiguration { get; }
    }
}
