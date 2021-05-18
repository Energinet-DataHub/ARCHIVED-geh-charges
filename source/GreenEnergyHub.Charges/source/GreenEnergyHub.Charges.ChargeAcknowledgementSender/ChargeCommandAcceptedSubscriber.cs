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
using GreenEnergyHub.Charges.Application.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeAcknowledgementSender
{
    public class ChargeCommandAcceptedSubscriber
    {
        private const string FunctionName = nameof(ChargeCommandAcceptedSubscriber);
        private readonly IChargeAcknowledgementSender _chargeAcknowledgementSender;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor<ChargeCommandAcceptedEvent> _messageExtractor;

        public ChargeCommandAcceptedSubscriber(
            IChargeAcknowledgementSender chargeAcknowledgementSender,
            ICorrelationContext correlationContext,
            MessageExtractor<ChargeCommandAcceptedEvent> messageExtractor)
        {
            _chargeAcknowledgementSender = chargeAcknowledgementSender;
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%COMMAND_ACCEPTED_TOPIC_NAME%",
            "%COMMAND_ACCEPTED_SUBSCRIPTION_NAME%",
            Connection = "COMMAND_ACCEPTED_LISTENER_CONNECTION_STRING")]
            byte[] message,
            ILogger log)
        {
            var acceptedEvent = await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            SetCorrelationContext(acceptedEvent);
            await _chargeAcknowledgementSender.HandleAsync(acceptedEvent).ConfigureAwait(false);

            log.LogDebug("Received event with correlation ID '{CorrelationId}'", acceptedEvent.CorrelationId);
        }

        private void SetCorrelationContext(ChargeCommandAcceptedEvent acceptedEvent)
        {
            _correlationContext.CorrelationId = acceptedEvent.CorrelationId;
        }
    }
}
