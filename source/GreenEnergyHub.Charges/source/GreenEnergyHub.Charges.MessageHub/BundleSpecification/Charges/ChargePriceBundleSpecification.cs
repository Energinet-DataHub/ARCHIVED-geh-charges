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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges
{
    public class ChargePriceBundleSpecification : BundleSpecification<AvailableChargePriceData, ChargePriceOperationsAcceptedEvent>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle
        /// for a single charge (charge price header data only)
        /// </summary>
        public const decimal ChargeMessageWeight = 5m;

        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle
        /// for a single price point
        /// </summary>
        public const decimal ChargePointMessageWeight = 0.2m;

        public ChargePriceBundleSpecification()
            : base(BundleType.ChargePriceDataAvailable)
        {
        }

        public override int GetMessageWeight(AvailableChargePriceData data)
        {
            return (int)Math.Round(
                (data.Points.Count * ChargePointMessageWeight) + ChargeMessageWeight,
                MidpointRounding.AwayFromZero);
        }
    }
}
