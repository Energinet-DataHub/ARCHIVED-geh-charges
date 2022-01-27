﻿// Copyright 2020 Energinet DataHub A/S
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

using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures.WebApi
{
    public static class WebApiConfigurationBuilder
    {
        /// <summary>
        /// Load settings from "local.settings.json" if available, but also allow
        /// those settings to be overriden using environment variables.
        /// </summary>
        public static IConfiguration BuildLocalSettingsConfiguration()
        {
            var localSettingsConfiguration = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return localSettingsConfiguration;
        }
    }
}
