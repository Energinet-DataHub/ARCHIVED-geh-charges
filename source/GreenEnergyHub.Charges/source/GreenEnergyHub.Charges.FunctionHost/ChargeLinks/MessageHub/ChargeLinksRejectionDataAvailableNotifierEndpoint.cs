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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Persistence;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks.MessageHub
{
    public class ChargeLinksRejectionDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargeLinksRejectionDataAvailableNotifierEndpoint);
        private readonly IAvailableDataNotifier<AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent> _availableDataNotifier;
        private readonly JsonMessageDeserializer<ChargeLinksRejectedEvent> _deserializer;
        private readonly IMessageHubUnitOfWork _messageHubUnitOfWork;

        public ChargeLinksRejectionDataAvailableNotifierEndpoint(
            IAvailableDataNotifier<AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent> availableDataNotifier,
            JsonMessageDeserializer<ChargeLinksRejectedEvent> deserializer,
            IMessageHubUnitOfWork messageHubUnitOfWork)
        {
            _availableDataNotifier = availableDataNotifier;
            _deserializer = deserializer;
            _messageHubUnitOfWork = messageHubUnitOfWork;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinksCommandRejectedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var rejectedEvent = (ChargeLinksRejectedEvent)await _deserializer.FromBytesAsync(message).ConfigureAwait(false);
            await _availableDataNotifier.NotifyAsync(rejectedEvent).ConfigureAwait(false);

            await _messageHubUnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
