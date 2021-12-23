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
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with MessageHub
    /// by notifying that charge and/or charge price data is available
    /// This is the RSM-034 CIM XML 'NotifyPriceList'.
    /// </summary>
    public class ChargeDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargeDataAvailableNotifierEndpoint);
        private readonly MessageExtractor<ChargeCommandAcceptedContract> _messageExtractor;
        private readonly IAvailableDataNotifier<AvailableChargeData, ChargeCommandAcceptedEvent> _availableDataNotifier;

        public ChargeDataAvailableNotifierEndpoint(
            MessageExtractor<ChargeCommandAcceptedContract> messageExtractor,
            IAvailableDataNotifier<AvailableChargeData, ChargeCommandAcceptedEvent> availableDataNotifier)
        {
            _messageExtractor = messageExtractor;
            _availableDataNotifier = availableDataNotifier;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.CommandAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var chargeCommandAcceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _availableDataNotifier.NotifyAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
