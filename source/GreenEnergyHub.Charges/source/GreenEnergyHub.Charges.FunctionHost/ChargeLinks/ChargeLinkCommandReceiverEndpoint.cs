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
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinkCommandReceiverEndpoint
    {
        public const string FunctionName = nameof(ChargeLinkCommandReceiverEndpoint);
        private readonly MessageExtractor<ChargeLinkCommandReceivedContract> _messageExtractor;
        private readonly IChargeLinkCommandReceivedHandler _chargeLinkCommandReceivedHandler;
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkCommandReceiverEndpoint(
            MessageExtractor<ChargeLinkCommandReceivedContract> messageExtractor,
            ICorrelationContext correlationContext,
            IChargeLinkCommandReceivedHandler chargeLinkCommandReceivedHandler)
        {
            _messageExtractor = messageExtractor;
            _correlationContext = correlationContext;
            _chargeLinkCommandReceivedHandler = chargeLinkCommandReceivedHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CHARGE_LINK_RECEIVED_TOPIC_NAME%",
                "%CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME%",
                Connection = "DOMAINEVENT_LISTENER_CONNECTION_STRING")]
            byte[] data,
            [NotNull] FunctionContext context)
        {
            SetupCorrelationContext(context);

            var chargeLinkCommandMessage =
                await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            await _chargeLinkCommandReceivedHandler
                .HandleAsync((ChargeLinkCommandReceivedEvent)chargeLinkCommandMessage).ConfigureAwait(false);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
