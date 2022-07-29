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
using System.ComponentModel;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Domain.AvailableData.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.MessageHub
{
    public static class BundleTypeMapper
    {
        private const string ChargeDataAvailable = "ChargeDataAvailable";
        private const string ChargeConfirmationDataAvailable = "ChargeConfirmationDataAvailable";
        private const string ChargeRejectionDataAvailable = "ChargeRejectionDataAvailable";
        private const string ChargeLinkDataAvailable = "ChargeLinkDataAvailable";
        private const string ChargeLinkConfirmationDataAvailable = "ChargeLinkConfirmationDataAvailable";
        private const string ChargeLinkRejectionDataAvailable = "ChargeLinkRejectionDataAvailable";

        public static string Map(BundleType bundleType)
        {
            return bundleType switch
            {
                BundleType.ChargeDataAvailable => ChargeDataAvailable,
                BundleType.ChargeConfirmationDataAvailable => ChargeConfirmationDataAvailable,
                BundleType.ChargeRejectionDataAvailable => ChargeRejectionDataAvailable,
                BundleType.ChargeLinkDataAvailable => ChargeLinkDataAvailable,
                BundleType.ChargeLinkConfirmationDataAvailable => ChargeLinkConfirmationDataAvailable,
                BundleType.ChargeLinkRejectionDataAvailable => ChargeLinkRejectionDataAvailable,
                _ => throw new InvalidEnumArgumentException(
                    $"Provided BundleType value '{bundleType}' is invalid and cannot be mapped"),
            };
        }

        public static BundleType Map(DataBundleRequestDto request)
        {
            var index = request.MessageType.Value.IndexOf('_');

            if (index < 0)
            {
                throw new ArgumentException($"Provided request.MessageType value '{request.MessageType.Value}' is invalid");
            }

            return Map(request.MessageType.Value.Substring(0, index));
        }

        public static BundleType Map(string bundleType)
        {
            return bundleType switch
            {
                ChargeDataAvailable => BundleType.ChargeDataAvailable,
                ChargeConfirmationDataAvailable => BundleType.ChargeConfirmationDataAvailable,
                ChargeRejectionDataAvailable => BundleType.ChargeRejectionDataAvailable,
                ChargeLinkDataAvailable => BundleType.ChargeLinkDataAvailable,
                ChargeLinkConfirmationDataAvailable => BundleType.ChargeLinkConfirmationDataAvailable,
                ChargeLinkRejectionDataAvailable => BundleType.ChargeLinkRejectionDataAvailable,
                _ => throw new ArgumentException(
                    $"Provided string value '{bundleType}' cannot be mapped to a BundleType"),
            };
        }
    }
}
