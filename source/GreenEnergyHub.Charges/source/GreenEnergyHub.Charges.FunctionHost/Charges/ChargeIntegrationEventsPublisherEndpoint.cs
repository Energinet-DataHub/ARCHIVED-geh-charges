﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeCommandAcceptedReceiverEndpoint
    {
        public const string FunctionName = nameof(ChargeCommandAcceptedReceiverEndpoint);
        private readonly MessageExtractor<ChargeCommandAcceptedContract> _messageExtractor;
        private readonly IChargeIntegrationEventsPublisher _chargeIntegrationEventsPublisher;

        public ChargeCommandAcceptedReceiverEndpoint(
            MessageExtractor<ChargeCommandAcceptedContract> messageExtractor,
            IChargeIntegrationEventsPublisher chargeIntegrationEventsPublisher)
        {
            _messageExtractor = messageExtractor;
            _chargeIntegrationEventsPublisher = chargeIntegrationEventsPublisher;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.CommandAcceptedTopicName + "%",
                "%" + EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var chargeCommandAcceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor
                .ExtractAsync(message)
                .ConfigureAwait(false);

            await _chargeIntegrationEventsPublisher.PublishAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
