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
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Charges.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeCommandReceiver
{
    public class ChargeCommandEndpoint
    {
        private const string FunctionName = nameof(ChargeCommandEndpoint);
        private readonly IChargeCommandReceivedEventHandler _chargeCommandReceivedEventHandler;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;

        public ChargeCommandEndpoint(
            IChargeCommandReceivedEventHandler chargeCommandReceivedEventHandler,
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor)
        {
            _chargeCommandReceivedEventHandler = chargeCommandReceivedEventHandler;
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%COMMAND_RECEIVED_TOPIC_NAME%",
            "%COMMAND_RECEIVED_SUBSCRIPTION_NAME%",
            Connection = "COMMAND_RECEIVED_LISTENER_CONNECTION_STRING")]
            byte[] data,
            ILogger log)
        {
            var receivedEvent = (ChargeCommandReceivedEvent)await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            SetCorrelationContext(receivedEvent);
            await _chargeCommandReceivedEventHandler.HandleAsync(receivedEvent).ConfigureAwait(false);

            log.LogDebug("Received command with charge ID '{ID}'", receivedEvent.Command.ChargeOperation.ChargeId);
        }

        private void SetCorrelationContext(ChargeCommandReceivedEvent command)
        {
            _correlationContext.CorrelationId = command.CorrelationId;
        }
    }
}
