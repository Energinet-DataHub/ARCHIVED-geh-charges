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
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;

namespace GreenEnergyHub.Charges.Infrastructure.MessageHub
{
    public class BundleCreatorProvider : IBundleCreatorProvider
    {
        private readonly Dictionary<Type, IBundleCreator> _bundleCreators;

        public BundleCreatorProvider(IEnumerable<IBundleCreator> bundleCreators)
        {
            _bundleCreators = bundleCreators.ToDictionary(creator => creator.GetType());
        }

        public IBundleCreator Get(DataBundleRequestDto request)
        {
            // RSM-034 CIM XML 'NotifyPriceList' requests
            if (request.MessageType.StartsWith(ChargeDataAvailableNotifier.MessageTypePrefix))
                return _bundleCreators[typeof(BundleCreator<AvailableChargeData>)];

            // RSM-030 CIM XML 'ConfirmRequestChangeBillingMasterData' confirmations
            if (request.MessageType.StartsWith(ChargeLinkConfirmationNotificationFactory.MessageTypePrefix))
                return _bundleCreators[typeof(BundleCreator<AvailableChargeLinkReceiptData>)];

            // RSM-031 CIM XML 'NotifyBillingMasterData' requests
            if (request.MessageType.StartsWith(ChargeLinksNotificationFactory.MessageTypePrefix))
                return _bundleCreators[typeof(BundleCreator<AvailableChargeLinksData>)];

            throw new ArgumentException(
                $"Unknown message type: {request.MessageType} with DataAvailableNotificationIds: {request.IdempotencyId}");
        }
    }
}
