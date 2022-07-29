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

using GreenEnergyHub.Charges.Domain.AvailableData.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableData;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;

namespace GreenEnergyHub.Charges.MessageHub.BundleSpecification.ChargeLinks
{
    public class ChargeLinksConfirmationBundleSpecification : BundleSpecification<AvailableChargeLinksReceiptData, ChargeLinksAcceptedEvent>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle
        /// for a single confirmation
        /// </summary>
        public const int MessageWeight = 2;

        public ChargeLinksConfirmationBundleSpecification()
            : base(BundleType.ChargeLinkConfirmationDataAvailable)
        {
        }

        public override int GetMessageWeight(AvailableChargeLinksReceiptData data)
        {
            return MessageWeight;
        }
    }
}
