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
using GreenEnergyHub.Charges.Domain.Dtos.Events;
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
        private const string FunctionName = nameof(ChargeRejectionDataAvailableNotifierEndpoint);
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, ChargeInformationOperationsRejectedEvent> _availableDataNotifier;
        private readonly JsonMessageDeserializer<ChargeInformationOperationsRejectedEvent> _deserializer;

        public ChargeRejectionDataAvailableNotifierEndpoint(
            IAvailableDataNotifier<AvailableChargeReceiptData, ChargeInformationOperationsRejectedEvent> availableDataNotifier,
            JsonMessageDeserializer<ChargeInformationOperationsRejectedEvent> deserializer)
        {
            _availableDataNotifier = availableDataNotifier;
            _deserializer = deserializer;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeCommandRejectedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var rejectedEvent = (ChargeInformationOperationsRejectedEvent)await _deserializer.FromBytesAsync(message).ConfigureAwait(false);
            if (rejectedEvent.Document.BusinessReasonCode == BusinessReasonCode.UpdateChargePrices) return;

            await _availableDataNotifier.NotifyAsync(rejectedEvent).ConfigureAwait(false);
        }
    }
}
