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

using System.ComponentModel;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketActivityRecord
{
    public static class ReasonCodeMapper
    {
        private const string CimIncorrectChargeInformation = "D14"; // Temporary ebix value until the real cim codes are known

        public static string Map(ReasonCode reasonCode)
        {
            return reasonCode switch
            {
                ReasonCode.IncorrectChargeInformation => CimIncorrectChargeInformation,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided ReasonCode value '{reasonCode}' is invalid and cannot be mapped."),
            };
        }
    }
}
