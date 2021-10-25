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
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub
{
    /// <summary>
    /// The function will initiate the communication with the post office
    /// by notifying that a charge link has been created.
    /// This is the RSM-031 CIM XML 'NotifyBillingMasterData'.
    /// </summary>
    public class ChargeDataAvailableNotifierEndpoint
    {
        private const string FunctionName = nameof(ChargeDataAvailableNotifierEndpoint);
        private readonly MessageExtractor<ChargeCommandAcceptedEvent> _messageExtractor;
        private readonly IChargeDataAvailableNotifier _chargeDataAvailableNotifier;
        private readonly ILogger _log;

        public ChargeDataAvailableNotifierEndpoint(
            MessageExtractor<ChargeCommandAcceptedEvent> messageExtractor,
            IChargeDataAvailableNotifier chargeDataAvailableNotifier,
            [NotNull] ILoggerFactory loggerFactory)
        {
            _messageExtractor = messageExtractor;
            _chargeDataAvailableNotifier = chargeDataAvailableNotifier;

            _log = loggerFactory.CreateLogger(nameof(ChargeDataAvailableNotifierEndpoint));
        }

        [Function(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CHARGE_ACCEPTED_TOPIC_NAME%",
                "%CHARGEACCEPTED_SUB_DATAAVAILABLENOTIFIER%",
                Connection = "DOMAINEVENT_LISTENER_CONNECTION_STRING")]
            [NotNull] byte[] message)
        {
            _log.LogInformation("Function {FunctionName} started to process a request with size {Size}", FunctionName, message.Length);

            var chargeCommandAcceptedEvent = (ChargeCommandAcceptedEvent)await _messageExtractor.ExtractAsync(message).ConfigureAwait(false);

            await _chargeDataAvailableNotifier.NotifyAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
