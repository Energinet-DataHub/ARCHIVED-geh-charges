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

namespace GreenEnergyHub.Charges.WebApi
{
    /// <summary>
    /// Contains names of settings used by the function.
    /// </summary>
    public static class EnvironmentSettingNames
    {
        // Function
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";

        // Environment specific settings
        public const string AppInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";
        public const string ChargeDbConnectionString = "CHARGE_DB_CONNECTION_STRING";

        // Identification of Hub
        public const string HubSenderId = "HUB_SENDER_ID";
        public const string HubSenderRoleIntEnumValue = "HUB_SENDER_ROLE_INT_ENUM_VALUE";

        // Localization
        public const string Currency = "CURRENCY";
        public const string LocalTimeZoneName = "LOCAL_TIMEZONENAME";
    }
}
