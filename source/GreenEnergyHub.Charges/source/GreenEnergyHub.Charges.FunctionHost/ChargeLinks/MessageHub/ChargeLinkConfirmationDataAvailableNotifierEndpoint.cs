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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandAccepted;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with the post office
    /// by notifying that a charge link has been created.
    /// This is the RSM-031 CIM XML 'NotifyBillingMasterData'.
    /// </summary>
    public class ChargeLinkConfirmationDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargeLinkConfirmationDataAvailableNotifierEndpoint);
        private readonly MessageExtractor<ChargeLinksCommandAccepted> _messageExtractor;
        private readonly IAvailableDataNotifier<AvailableChargeLinksReceiptData, ChargeLinksAcceptedEvent> _availableDataNotifier;

        public ChargeLinkConfirmationDataAvailableNotifierEndpoint(
            MessageExtractor<ChargeLinksCommandAccepted> messageExtractor,
            IAvailableDataNotifier<AvailableChargeLinksReceiptData, ChargeLinksAcceptedEvent> availableDataNotifier)
        {
            _messageExtractor = messageExtractor;
            _availableDataNotifier = availableDataNotifier;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargeLinksAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var chargeLinkCommandAcceptedEvent = (ChargeLinksAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _availableDataNotifier.NotifyAsync(chargeLinkCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
