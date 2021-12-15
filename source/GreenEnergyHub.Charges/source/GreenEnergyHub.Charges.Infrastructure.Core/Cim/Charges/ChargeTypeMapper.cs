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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundles.ChargeLinkBundle.Cim
{
    public static class ChargeTypeMapper
    {
        private const string CimFee = "D02";
        private const string CimSubscription = "D01";
        private const string CimTariff = "D03";

        public static ChargeType Map(string value)
        {
            return value switch
            {
                CimFee => ChargeType.Fee,
                CimSubscription => ChargeType.Subscription,
                CimTariff => ChargeType.Tariff,
                _ => ChargeType.Unknown,
            };
        }

        public static string Map(ChargeType chargeType)
        {
            return chargeType switch
            {
                ChargeType.Fee => CimFee,
                ChargeType.Subscription => CimSubscription,
                ChargeType.Tariff => CimTariff,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided ChargeType value '{chargeType}' is invalid and cannot be mapped."),
            };
        }
    }
}
