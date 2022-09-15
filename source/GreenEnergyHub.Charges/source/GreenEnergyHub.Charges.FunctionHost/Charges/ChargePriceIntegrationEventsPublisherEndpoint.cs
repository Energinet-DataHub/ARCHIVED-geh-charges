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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargePriceIntegrationEventsPublisherEndpoint
    {
        private const string FunctionName = nameof(ChargePriceIntegrationEventsPublisherEndpoint);
        private readonly JsonMessageDeserializer<ChargePriceOperationsConfirmedEvent> _deserializer;
        private readonly IChargePriceIntegrationEventsPublisher _chargePriceIntegrationEventsPublisher;

        public ChargePriceIntegrationEventsPublisherEndpoint(
            JsonMessageDeserializer<ChargePriceOperationsConfirmedEvent> deserializer,
            IChargePriceIntegrationEventsPublisher chargePriceIntegrationEventsPublisher)
        {
            _deserializer = deserializer;
            _chargePriceIntegrationEventsPublisher = chargePriceIntegrationEventsPublisher;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargesDomainEventTopicName + "%",
                "%" + EnvironmentSettingNames.ChargePriceConfirmedPublishSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var chargePriceOperationsConfirmedEvent = (ChargePriceOperationsConfirmedEvent)await _deserializer
                .FromBytesAsync(message).ConfigureAwait(false);
            await _chargePriceIntegrationEventsPublisher.PublishAsync(chargePriceOperationsConfirmedEvent).ConfigureAwait(false);
        }
    }
}
