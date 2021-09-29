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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinkEventPublisherEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(ChargeLinkEventPublisherEndpoint);
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<ChargeLinkCommandAcceptedContract> _messageExtractor;
        private readonly IChargeLinkEventPublishHandler _chargeLinkEventPublishHandler;
        private readonly ILogger _log;

        public ChargeLinkEventPublisherEndpoint(
            ICorrelationContext correlationContext,
            MessageExtractor<ChargeLinkCommandAcceptedContract> messageExtractor,
            IChargeLinkEventPublishHandler chargeLinkEventPublishHandler,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _chargeLinkEventPublishHandler = chargeLinkEventPublishHandler;

            _log = loggerFactory.CreateLogger(nameof(ChargeLinkEventPublisherEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CHARGE_LINK_ACCEPTED_TOPIC_NAME%",
                "%CHARGE_LINK_ACCEPTED_SUBSCRIPTION_NAME%",
                Connection = "DOMAINEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, message.Length);

            SetupCorrelationContext(context);

            var acceptedChargeLinkCommand = (ChargeLinkCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _chargeLinkEventPublishHandler.HandleAsync(acceptedChargeLinkCommand).ConfigureAwait(false);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId.Replace("-", string.Empty);
        }
    }
}
