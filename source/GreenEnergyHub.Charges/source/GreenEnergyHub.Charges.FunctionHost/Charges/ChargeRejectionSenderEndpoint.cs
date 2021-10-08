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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeCommandRejected;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges
{
    public class ChargeRejectionSenderEndpoint
    {
        public const string FunctionName = nameof(ChargeRejectionSenderEndpoint);
        private readonly IChargeRejectionSender _chargeRejectionSender;
        private readonly MessageExtractor<ChargeCommandRejectedContract> _messageExtractor;
        private readonly ILogger _log;

        public ChargeRejectionSenderEndpoint(
            IChargeRejectionSender chargeRejectionSender,
            MessageExtractor<ChargeCommandRejectedContract> messageExtractor,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _chargeRejectionSender = chargeRejectionSender;
            _messageExtractor = messageExtractor;

            _log = loggerFactory.CreateLogger(nameof(ChargeRejectionSenderEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%COMMAND_REJECTED_TOPIC_NAME%",
                "%COMMAND_REJECTED_SUBSCRIPTION_NAME%",
                Connection = EnvironmentSettingNames.DomainEventListenerConnectionString)]
            [NotNull] byte[] message)
        {
            var rejectedEvent = (ChargeCommandRejectedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);
            await _chargeRejectionSender.HandleAsync(rejectedEvent).ConfigureAwait(false);

            _log.LogDebug("Received event with correlation ID '{CorrelationId}'", rejectedEvent.CorrelationId);
        }
    }
}
