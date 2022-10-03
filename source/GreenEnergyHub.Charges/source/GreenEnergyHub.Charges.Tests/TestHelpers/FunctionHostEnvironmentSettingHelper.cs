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
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public static class FunctionHostEnvironmentSettingHelper
    {
        private static readonly string[] _serviceBusConnectionStrings = new[]
        {
            "DOMAINEVENT_LISTENER_CONNECTION_STRING",
            "DOMAINEVENT_MANAGER_CONNECTION_STRING",
            "DOMAINEVENT_SENDER_CONNECTION_STRING",
            "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING",
            "INTEGRATIONEVENT_MANAGER_CONNECTION_STRING",
            "INTEGRATIONEVENT_SENDER_CONNECTION_STRING",
        };

        public static void SetFunctionHostEnvironmentVariablesFromSampleSettingsFile(IConfiguration configuration)
        {
            var variables = configuration.GetSection("Values");
            foreach (var variable in variables.GetChildren())
            {
                var (key, value) = (variable.Key, variable.Value);

                if (_serviceBusConnectionStrings.Contains(variable.Key))
                {
                    value = "Endpoint=sb://sb-mybus.servicebus.windows.net/;SharedAccessKeyName=accessKeyName;SharedAccessKey=accessKey";
                }

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
