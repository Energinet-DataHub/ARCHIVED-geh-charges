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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandReceived;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeCommandReceiverEndpoint
    {
        public const string FunctionName = nameof(ChargeCommandReceiverEndpoint);
        private readonly IChargeCommandReceivedEventHandler _chargeCommandReceivedEventHandler;
        private readonly MessageExtractor<ChargeCommandReceivedEvent> _messageExtractor;

        public ChargeCommandReceiverEndpoint(
            IChargeCommandReceivedEventHandler chargeCommandReceivedEventHandler,
            MessageExtractor<ChargeCommandReceivedEvent> messageExtractor)
        {
            _chargeCommandReceivedEventHandler = chargeCommandReceivedEventHandler;
            _messageExtractor = messageExtractor;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.CommandReceivedTopicName + "%",
                "%" + EnvironmentSettingNames.CommandReceivedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] message)
        {
            var receivedEvent = (ChargeCommandReceivedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            await _chargeCommandReceivedEventHandler.HandleAsync(receivedEvent).ConfigureAwait(false);
        }
    }
}
