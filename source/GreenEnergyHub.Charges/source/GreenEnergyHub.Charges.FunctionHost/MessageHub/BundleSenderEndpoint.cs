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
using System.Threading.Tasks;
using Energinet.DataHub.MessageHub.Model.Peek;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Application.Charges.MessageHub;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.MessageHub
{
    /// <summary>
    /// Trigger on request from MessageHub to create a bundle
    /// and create bundle and send response to MessageHub.
    /// This is the RSM-034 CIM XML 'NotifyPriceList' and RSM-031 CIM XML 'NotifyBillingMasterData'.
    /// </summary>
    public class BundleSenderEndpoint
    {
        private const string FunctionName = nameof(BundleSenderEndpoint);
        private readonly IChargeBundleSender _chargeBundleSender;
        private readonly IChargeLinkBundleSender _chargeLinkBundleSender;
        private readonly IRequestBundleParser _requestBundleParser;

        public BundleSenderEndpoint(
            IChargeBundleSender chargeBundleSender,
            IChargeLinkBundleSender chargeLinkBundleSender,
            IRequestBundleParser requestBundleParser)
        {
            _chargeBundleSender = chargeBundleSender;
            _chargeLinkBundleSender = chargeLinkBundleSender;
            _requestBundleParser = requestBundleParser;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.MessageHubRequestQueue + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString,
                IsSessionsEnabled = true)]
            byte[] data)
        {
            var request = _requestBundleParser.Parse(data);

            if (request.MessageType.StartsWith(ChargeDataAvailableNotifier.ChargeDataAvailableMessageTypePrefix))
                await _chargeBundleSender.SendAsync(request).ConfigureAwait(false);
            if (request.MessageType.StartsWith(ChargeLinkDataAvailableNotifier.ChargeLinkDataAvailableMessageTypePrefix))
                await _chargeLinkBundleSender.SendAsync(request).ConfigureAwait(false);
            throw new ArgumentException(
                $"Unknown message type: {request.MessageType} with DataAvailableNotificationIds: {request.DataAvailableNotificationIds}");
        }
    }
}
