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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeCommandAcceptedReceiverEndpoint
    {
        public const string FunctionName = nameof(ChargeCommandAcceptedReceiverEndpoint);
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<ChargeCommandAcceptedEvent> _messageExtractor;
        private readonly IChargeCommandAcceptedEventHandler _chargeCommandAcceptedEventHandler;

        public ChargeCommandAcceptedReceiverEndpoint(
            ICorrelationContext correlationContext,
            MessageExtractor<ChargeCommandAcceptedEvent> messageExtractor,
            IChargeCommandAcceptedEventHandler chargeCommandAcceptedEventHandler)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _chargeCommandAcceptedEventHandler = chargeCommandAcceptedEventHandler;
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%COMMAND_ACCEPTED_TOPIC_NAME%",
                "%COMMAND_ACCEPTED_RECEIVER_SUBSCRIPTION_NAME%",
                Connection = "COMMAND_ACCEPTED_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message,
            [NotNull] FunctionContext context)
        {
            SetupCorrelationContext(context); // TODO Add this as a method in correlation context instead once integration project has been upgraded to .5.0, avoiding multiple of the same implementations

            var chargeCommandAcceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor
                .ExtractAsync(message)
                .ConfigureAwait(false);

            await _chargeCommandAcceptedEventHandler.HandleAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId.Replace("-", string.Empty);
        }
    }
}
