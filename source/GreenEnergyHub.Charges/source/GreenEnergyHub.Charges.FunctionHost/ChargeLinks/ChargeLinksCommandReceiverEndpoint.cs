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
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinksCommandReceiverEndpoint
    {
        public const string FunctionName = nameof(ChargeLinksCommandReceiverEndpoint);
        private readonly MessageExtractor<ChargeLinkCommandReceived> _messageExtractor;
        private readonly IChargeLinksReceivedEventHandler _chargeLinksReceivedEventHandler;

        public ChargeLinksCommandReceiverEndpoint(
            MessageExtractor<ChargeLinkCommandReceived> messageExtractor,
            IChargeLinksReceivedEventHandler chargeLinksReceivedEventHandler)
        {
            _messageExtractor = messageExtractor;
            _chargeLinksReceivedEventHandler = chargeLinksReceivedEventHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.ChargeLinkReceivedTopicName + "%",
                "%" + EnvironmentSettingNames.ChargeLinkReceivedSubscriptionName + "%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            byte[] data)
        {
            var chargeLinkReceivedEvent =
                await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            await _chargeLinksReceivedEventHandler
                .HandleAsync((ChargeLinksReceivedEvent)chargeLinkReceivedEvent).ConfigureAwait(false);
        }
    }
}
