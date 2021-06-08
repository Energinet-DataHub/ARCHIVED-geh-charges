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

using System.Collections;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class EnvironmentVariableReader
    {
        private readonly IDictionary _environmentVariables;

        public EnvironmentVariableReader(IDictionary environmentVariables)
        {
            _environmentVariables = environmentVariables;
        }

        public string GetEnvironmentVariableOrEmptyString(string variableName)
        {
            return _environmentVariables[variableName]?.ToString() ?? string.Empty;
        }

        public bool GetEnvironmentVariableOrFalse(string variableName)
        {
            return _environmentVariables[variableName]?.ToString()?.ToUpperInvariant() == "TRUE";
        }

        public bool GetEnvironmentVariableOrTrue(string variableName)
        {
            return _environmentVariables[variableName]?.ToString()?.ToUpperInvariant() != "FALSE";
        }
    }
}
