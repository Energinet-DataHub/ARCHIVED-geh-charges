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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MeteringPointCreatedReceiver
{
    public class MeteringPointCreatedReceiverEndpoint
    {
        /// <summary>
        /// The name of the function.
        /// Function name affects the URL and thus possibly dependent infrastructure.
        /// </summary>
        private const string FunctionName = nameof(MeteringPointCreatedReceiverEndpoint);
        private readonly MessageExtractor _messageExtractor;
        private readonly IMeteringPointCreatedEventHandler _meteringPointCreatedEventHandler;

        public MeteringPointCreatedReceiverEndpoint(MessageExtractor messageExtractor, IMeteringPointCreatedEventHandler meteringPointCreatedEventHandler)
        {
            _messageExtractor = messageExtractor;
            _meteringPointCreatedEventHandler = meteringPointCreatedEventHandler;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%METERING_POINT_CREATED_TOPIC_NAME%",
                "%METERING_POINT_CREATED_SUBSCRIPTION_NAME%",
                Connection = "METERING_POINT_CREATED_LISTENER_CONNECTION_STRING")]
            byte[] data,
            ILogger log)
        {
            var meteringPointCreatedEvent = (MeteringPointCreatedEvent)await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);

            await _meteringPointCreatedEventHandler.HandleAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            log.LogInformation("Received metering point created event '{@MeteringPointId}'", meteringPointCreatedEvent.MeteringPointId);
        }
    }
}
