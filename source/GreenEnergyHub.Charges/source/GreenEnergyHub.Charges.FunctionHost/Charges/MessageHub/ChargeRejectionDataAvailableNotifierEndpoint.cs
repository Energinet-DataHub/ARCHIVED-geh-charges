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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    public class ChargeRejectionDataAvailableNotifierEndpoint
    {
        public const string FunctionName = nameof(ChargeRejectionDataAvailableNotifierEndpoint);
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandRejectedEvent> _availableDataNotifier;
        private readonly JsonMessageDeserializer<ChargeCommandRejectedEvent> _deserializer;

        public ChargeRejectionDataAvailableNotifierEndpoint(
            IAvailableDataNotifier<AvailableChargeReceiptData, ChargeCommandRejectedEvent> availableDataNotifier,
            JsonMessageDeserializer<ChargeCommandRejectedEvent> deserializer)
        {
            _availableDataNotifier = availableDataNotifier;
            _deserializer = deserializer;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesTopicName + "%",
                "%" + EnvironmentSettingNames.CommandRejectedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var rejectedEvent = (ChargeCommandRejectedEvent)await _deserializer.FromBytesAsync(message).ConfigureAwait(false);
            if (rejectedEvent.Command.Document.BusinessReasonCode == BusinessReasonCode.UpdateChargePrices) return;

            await _availableDataNotifier.NotifyAsync(rejectedEvent).ConfigureAwait(false);
        }
    }
}
