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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.ChargeRejectionSender
{
    public class ChargeCommandRejectedSubscriber
    {
        public const string FunctionName = nameof(ChargeCommandRejectedSubscriber);
        private readonly IChargeRejectionSender _chargeRejectionSender;
        private readonly ICorrelationContext _correlationContext;
        private readonly MessageExtractor _messageExtractor;

        public ChargeCommandRejectedSubscriber(
            IChargeRejectionSender chargeRejectionSender,
            ICorrelationContext correlationContext,
            MessageExtractor messageExtractor)
        {
            _chargeRejectionSender = chargeRejectionSender;
            _correlationContext = correlationContext;
            _messageExtractor = messageExtractor;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
            "%COMMAND_REJECTED_TOPIC_NAME%",
            "%COMMAND_REJECTED_SUBSCRIPTION_NAME%",
            Connection = "COMMAND_REJECTED_LISTENER_CONNECTION_STRING")]
            byte[] message,
            ILogger log)
        {
            var rejectedEvent = (ChargeCommandRejectedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            SetCorrelationContext(rejectedEvent);
            await _chargeRejectionSender.HandleAsync(rejectedEvent).ConfigureAwait(false);

            log.LogDebug("Received event with correlation ID '{CorrelationId}'", rejectedEvent.CorrelationId);
        }

        private void SetCorrelationContext(ChargeCommandRejectedEvent rejectedEvent)
        {
            _correlationContext.CorrelationId = rejectedEvent.CorrelationId;
        }
    }
}
