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
using GreenEnergyHub.Charges.Domain.Charges.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHosts
{
    public class MeteringPointCreatedReceiverEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = nameof(MeteringPointCreatedReceiverEndpoint);
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;
        private readonly IMeteringPointCreatedEventHandler _meteringPointCreatedEventHandler;
        private readonly ILogger _log;

        public MeteringPointCreatedReceiverEndpoint(
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor,
            IMeteringPointCreatedEventHandler meteringPointCreatedEventHandler,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
            _meteringPointCreatedEventHandler = meteringPointCreatedEventHandler;
            _log = loggerFactory.CreateLogger(nameof(MeteringPointCreatedReceiverEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%METERING_POINT_CREATED_TOPIC_NAME%",
                "%METERING_POINT_CREATED_SUBSCRIPTION_NAME%",
                Connection = "INTEGRATIONEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message,
            [NotNull] FunctionContext context)
        {
            SetupCorrelationContext(context); // TODO Add this as a method in correlation context instead once integration project has been upgraded to .5.0, avoiding multiple of the same implementations

            var meteringPointCreatedEvent = (MeteringPointCreatedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _meteringPointCreatedEventHandler.HandleAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            _log.LogInformation("Received metering point created event '{@MeteringPointId}'", meteringPointCreatedEvent.MeteringPointId);
        }

        private void SetupCorrelationContext(FunctionContext context)
        {
            _correlationContext.CorrelationId = context.InvocationId.Replace("-", string.Empty);
        }
    }
}
