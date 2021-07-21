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
using GreenEnergyHub.Charges.Application.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.WebJobs;

namespace GreenEnergyHub.Charges.ChargeLinkCommandReceiver
{
    public class LinkCommandReceiverEndpoint
    {
        private const string FunctionName = nameof(LinkCommandReceiverEndpoint);
        private readonly MessageExtractor _messageExtractor;
        private readonly IChargeLinkCommandAcceptedHandler _chargeLinkCommandAcceptedHandler;

        public LinkCommandReceiverEndpoint(
            MessageExtractor messageExtractor,
            IChargeLinkCommandAcceptedHandler chargeLinkCommandAcceptedHandler)
        {
            _messageExtractor = messageExtractor;
            _chargeLinkCommandAcceptedHandler = chargeLinkCommandAcceptedHandler;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync(
            [ServiceBusTrigger(
                "%CHARGE_LINK_RECEIVED_TOPIC_NAME%",
                "%CHARGE_LINK_RECEIVED_SUBSCRIPTION_NAME%",
                Connection = "CHARGE_LINK_RECEIVED_LISTENER_CONNECTION_STRING")]
            byte[] data)
        {
            var chargeLinkCommandMessage =
                await _messageExtractor.ExtractAsync(data).ConfigureAwait(false);
            await _chargeLinkCommandAcceptedHandler.HandleAsync((ChargeCommandReceivedEvent)chargeLinkCommandMessage);
        }
    }
}
