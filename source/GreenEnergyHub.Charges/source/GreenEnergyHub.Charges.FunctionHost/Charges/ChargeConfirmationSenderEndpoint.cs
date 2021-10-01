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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeConfirmationSenderEndpoint
    {
        public const string FunctionName = nameof(ChargeConfirmationSenderEndpoint);
        private readonly IChargeConfirmationSender _chargeConfirmationSender;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<ChargeCommandAcceptedContract> _messageExtractor;
        private readonly ILogger _log;

        public ChargeConfirmationSenderEndpoint(
            IChargeConfirmationSender chargeConfirmationSender,
            ICorrelationContext correlationContext,
            MessageExtractor<ChargeCommandAcceptedContract> messageExtractor,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _chargeConfirmationSender = chargeConfirmationSender;
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;

            _log = loggerFactory.CreateLogger(nameof(ChargeConfirmationSenderEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%COMMAND_ACCEPTED_TOPIC_NAME%",
                "%COMMAND_ACCEPTED_SUBSCRIPTION_NAME%",
                Connection = "DOMAINEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message)
        {
            var acceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            await _chargeConfirmationSender.HandleAsync(acceptedEvent).ConfigureAwait(false);

            _log.LogDebug("Received event with correlation ID '{CorrelationId}'", acceptedEvent.CorrelationId);
        }
    }
}
