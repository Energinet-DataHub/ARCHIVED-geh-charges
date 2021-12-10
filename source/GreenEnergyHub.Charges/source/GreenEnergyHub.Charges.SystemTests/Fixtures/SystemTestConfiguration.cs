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
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    public class SystemTestConfiguration
    {
        public SystemTestConfiguration()
        {
            Configuration = BuildConfiguration();

            ShouldSkip = Configuration.GetValue<bool>("SYSTEMFACT_SKIP", defaultValue: true);
            BaseAddress = new Uri(Configuration.GetValue<string>("MYDOMAIN_BASEADDRESS"));
        }

        public bool ShouldSkip { get; }

        public Uri BaseAddress { get; }

        private IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Load settings from file if available, but also allow
        /// those settings to be overriden using environment variables.
        /// </summary>
        private IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("systemtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
