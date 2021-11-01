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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Commands;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.MeteringPoint
{
    public class CreateChargeLinkReceiverEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(CreateChargeLinkReceiverEndpoint);
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<CreateDefaultChargeLinks> _messageExtractor;
        private readonly ILogger _log;
        private readonly ICreateLinkCommandRequestHandler _createLinkCommandRequestHandler;

        public CreateChargeLinkReceiverEndpoint(
            ICorrelationContext correlationContext,
            MessageExtractor<CreateDefaultChargeLinks> messageExtractor,
            [NotNull] ILoggerFactory loggerFactory,
            ICreateLinkCommandRequestHandler createLinkCommandRequestHandler)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _createLinkCommandRequestHandler = createLinkCommandRequestHandler;
            _log = loggerFactory.CreateLogger(nameof(CreateChargeLinkReceiverEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CREATE_LINK_REQUEST_QUEUE_NAME%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            [NotNull] byte[] message)
        {
            _log.LogInformation(
                "Function {FunctionName} started to process a request with size {Size}",
                FunctionName,
                message.Length);

            var createLinkCommandEvent =
                (CreateLinkCommandEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _createLinkCommandRequestHandler
                .HandleAsync(createLinkCommandEvent, _correlationContext.Id).ConfigureAwait(false);

            _log.LogInformation(
                "Received create link command for metering point id '{MeteringPointId}'",
                createLinkCommandEvent.MeteringPointId);
        }
    }
}
