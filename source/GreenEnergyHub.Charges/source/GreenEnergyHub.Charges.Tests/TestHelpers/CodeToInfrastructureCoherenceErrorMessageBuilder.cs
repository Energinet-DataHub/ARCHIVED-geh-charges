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

using System.Text;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.FunctionHost.Common;

namespace GreenEnergyHub.Charges.Tests.TestHelpers
{
    public static class CodeToInfrastructureCoherenceErrorMessageBuilder
    {
        public static string CreateErrorMessage(string? setting = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The failure of this test indicates a discrepancy between one or more elements " +
                          "that must be coherent. Please ensure conformity in the following elements:");
            sb.AppendLine($" - inheritances of '{nameof(DomainEvent)}'");
            sb.AppendLine($" - '{nameof(EnvironmentSettingNames)}");
            sb.AppendLine(" - 'local.settings.sample.json'");
            sb.AppendLine(" - 'ChargesServiceBusResourceNames'");
            sb.AppendLine(" - 'func-functionhost.tf'");
            sb.AppendLine(" - 'sbt-charges-domain-events.tf'");
            if (setting != null) sb.AppendLine($" - setting not found: {setting}");

            return sb.ToString();
        }
    }
}
