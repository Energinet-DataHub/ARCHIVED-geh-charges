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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.BundleSpecification.ChargeLinks
{
    public class ChargeLinksRejectionBundleSpecification : BundleSpecification<AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent>
    {
        /// <summary>
        /// The upper anticipated weight (kilobytes) contribution to the final bundle
        /// for a single rejection without reasons
        /// </summary>
        public const decimal RejectionWeight = 2;

        /// <summary>
        /// The upper anticipated weight (kilobytes) of a single reason with the length
        /// of the reason text
        /// </summary>
        public const decimal RejectionReasonWeight = 0.1m;

        public ChargeLinksRejectionBundleSpecification(BundleType bundleType)
            : base(bundleType)
        {
        }

        public override int GetMessageWeight(AvailableChargeLinksReceiptData data)
        {
            // Note: Unlike the other fields in the CIM documents, the text field of the rejection is uncapped in size
            return (int)Math.Round(
                (data.ValidationErrors.Count * RejectionReasonWeight)
                + RejectionWeight
                + data.ValidationErrors.Sum(reason => reason.Text.Length),
                MidpointRounding.AwayFromZero);
        }
    }
}
