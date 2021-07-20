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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeLinkEventPublisher
{
    public class ChargeLinkEventPublisherServiceBusTrigger
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = "ChargeLinkEventPublisherServiceBusTrigger";
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;
        private readonly ILogger _log;

        public ChargeLinkEventPublisherServiceBusTrigger(
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;

            _log = loggerFactory.CreateLogger(nameof(ChargeLinkEventPublisherServiceBusTrigger));
        }

        [Function(FunctionName)]
        public async Task<IActionResult> RunAsync(
            [ServiceBusTrigger(
                "%LINK_ACCEPTED_TOPIC_NAME%",
                "%LINK_ACCEPTED_SUBSCRIPTION_NAME%",
                Connection = "LINK_ACCEPTED_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message,
            [NotNull] FunctionContext context)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, message.Length);

            SetupCorrelationContext(context);

            var acceptedChargeLinkCommand = (ChargeLinkCommand)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            _log.LogInformation("Received charge link accepted event with document id '{@DocumentId}'", acceptedChargeLinkCommand.Document.Id);

            return new OkResult();
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId;
        }
    }
}
