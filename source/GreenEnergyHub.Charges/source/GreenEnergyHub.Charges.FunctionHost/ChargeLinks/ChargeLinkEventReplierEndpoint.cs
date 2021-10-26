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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.ChargeLinks
{
    public class ChargeLinkEventReplierEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        public const string FunctionName = nameof(ChargeLinkEventReplierEndpoint);
        private readonly MessageExtractor<ChargeLinkCommandAccepted> _messageExtractor;
        private readonly IChargeLinkEventReplyHandler _chargeLinkEventReplyHandler;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _log;

        public ChargeLinkEventReplierEndpoint(
            MessageExtractor<ChargeLinkCommandAccepted> messageExtractor,
            IChargeLinkEventReplyHandler chargeLinkEventReplyHandler,
            IMessageMetaDataContext messageMetaDataContext,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _messageExtractor = messageExtractor;
            _chargeLinkEventReplyHandler = chargeLinkEventReplyHandler;
            _messageMetaDataContext = messageMetaDataContext;

            _log = loggerFactory.CreateLogger(nameof(ChargeLinkEventReplierEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CHARGE_LINK_ACCEPTED_TOPIC_NAME%",
                "%CHARGELINKACCEPTED_SUB_REPLIER%",
                Connection = "DOMAINEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, message.Length);

            var acceptedChargeLinkCommand = (ChargeLinkCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            if (_messageMetaDataContext.IsReplyToSet())
                await _chargeLinkEventReplyHandler.HandleAsync(acceptedChargeLinkCommand).ConfigureAwait(false);
        }
    }
}
