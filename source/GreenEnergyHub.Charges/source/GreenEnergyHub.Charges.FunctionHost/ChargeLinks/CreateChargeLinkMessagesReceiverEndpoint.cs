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
using Energinet.Charges.Contracts;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Commands;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.Domain.CreateLinkMessagesCommandEvent;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class CreateChargeLinkMessagesReceiverEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(CreateChargeLinkMessagesReceiverEndpoint);
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<CreateDefaultChargeLinkMessages> _messageExtractor;
        private readonly ICreateLinkMessagesCommandRequestHandler _createLinkMessagesCommandRequestHandler;
        private readonly ILogger _log;

        public CreateChargeLinkMessagesReceiverEndpoint(
            ICorrelationContext correlationContext,
            MessageExtractor<CreateDefaultChargeLinkMessages> messageExtractor,
            ICreateLinkMessagesCommandRequestHandler createLinkMessagesCommandRequestHandler,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _createLinkMessagesCommandRequestHandler = createLinkMessagesCommandRequestHandler;
            _log = loggerFactory.CreateLogger(nameof(CreateChargeLinkMessagesReceiverEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%" + EnvironmentSettingNames.CreateLinkMessagesRequestQueueName + "%",
                Connection = EnvironmentSettingNames.DataHubListenerConnectionString)]
            [NotNull] byte[] message)
        {
            _log.LogInformation(
                "Function {FunctionName} started to process a request with size {Size}",
                FunctionName,
                message.Length);

            var createLinkCommandEvent =
                (CreateLinkMessagesCommandEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _createLinkMessagesCommandRequestHandler.HandleAsync(createLinkCommandEvent, _correlationContext.Id)
                .ConfigureAwait(false);

            _log.LogInformation(
                "Received create link messages command for metering point id '{MeteringPointId}'",
                createLinkCommandEvent.MeteringPointId);
        }
    }
}
