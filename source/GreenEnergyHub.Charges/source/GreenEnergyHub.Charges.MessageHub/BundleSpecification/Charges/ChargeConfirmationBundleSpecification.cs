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

using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges
{
    public class ChargeConfirmationBundleSpecification : BundleSpecification<AvailableChargeReceiptData, ChargeInformationOperationsAcceptedEvent>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle
        /// for a single confirmation
        /// </summary>
        public const int MessageWeight = 2;

        public ChargeConfirmationBundleSpecification()
            : base(BundleType.ChargeConfirmationDataAvailable)
        {
        }

        public override int GetMessageWeight(AvailableChargeReceiptData data)
        {
            return MessageWeight;
        }
    }
}
