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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.MessageHub.Application.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with the message hub
    /// by notifying that a charge change has been confirmed
    /// This is the RSM-033 CIM XML 'ConfirmRequestChangeBillingMasterData'.
    /// </summary>
    public class ChargeConfirmationDataAvailableNotifierEndpoint
    {
        public const string FunctionName = nameof(ChargeConfirmationDataAvailableNotifierEndpoint);
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandAcceptedEvent> _availableDataNotifier;
        private readonly MessageExtractor<ChargeCommandAcceptedContract> _messageExtractor;

        public ChargeConfirmationDataAvailableNotifierEndpoint(
            IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandAcceptedEvent> availableDataNotifier,
            MessageExtractor<ChargeCommandAcceptedContract> messageExtractor)
        {
            _availableDataNotifier = availableDataNotifier;
            _messageExtractor = messageExtractor;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.CommandAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.CommandAcceptedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var acceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            await _availableDataNotifier.NotifyAsync(acceptedEvent).ConfigureAwait(false);
        }
    }
}
