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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.CreateLinkCommandReceiver
{
    public class CreateLinkCommandReceiverServiceBusTrigger
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "CreateLinkCommandReceiverServiceBusTrigger";
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;
        private readonly ILogger _log;
        private readonly ICreateLinkCommandEventHandler _createLinkCommandEventHandler;

        public CreateLinkCommandReceiverServiceBusTrigger(
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor,
            [NotNull] ILoggerFactory loggerFactory,
            ICreateLinkCommandEventHandler createLinkCommandEventHandler)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _createLinkCommandEventHandler = createLinkCommandEventHandler;

            _log = loggerFactory.CreateLogger(nameof(CreateLinkCommandReceiverServiceBusTrigger));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CREATE_LINK_COMMAND_TOPIC_NAME%",
                "%CREATE_LINK_COMMAND_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation(
                "Function {FunctionName} started to process a request with size {Size}",
                FunctionName,
                message.Length);

            SetupCorrelationContext(context);

            var createLinkCommandEvent =
                (CreateLinkCommandEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _createLinkCommandEventHandler
                .HandleAsync(createLinkCommandEvent, _correlationContext.CorrelationId).ConfigureAwait(false);

            _log.LogInformation(
                "Received create link command for metering point id '{MeteringPointId}' of type '{MeteringPointType}' on '{StartDateTime}'",
                createLinkCommandEvent.MeteringPointId,
                createLinkCommandEvent.MeteringPointType,
                createLinkCommandEvent.StartDateTime);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
