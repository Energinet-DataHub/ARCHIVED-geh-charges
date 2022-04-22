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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument
{
    public static class BusinessReasonCodeMapper
    {
        private const string CimUpdatePriceInformation = "D08";
        private const string CimUpdateMasterDataSettlement = "D17";
        private const string CimUpdateChargeInformation = "D18";

        public static BusinessReasonCode Map(string value)
        {
            return value switch
            {
                CimUpdatePriceInformation => BusinessReasonCode.UpdatePriceInformation,
                CimUpdateMasterDataSettlement => BusinessReasonCode.UpdateMasterDataSettlement,
                CimUpdateChargeInformation => BusinessReasonCode.UpdateChargeInformation,
                _ => BusinessReasonCode.Unknown,
            };
        }

        public static string Map(BusinessReasonCode businessReasonCode)
        {
            return businessReasonCode switch
            {
                BusinessReasonCode.UpdatePriceInformation => CimUpdatePriceInformation,
                BusinessReasonCode.UpdateChargeInformation => CimUpdateChargeInformation,
                BusinessReasonCode.UpdateMasterDataSettlement => CimUpdateMasterDataSettlement,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided BusinessReasonCode value '{businessReasonCode}' is invalid and cannot be mapped."),
            };
        }
    }
}
