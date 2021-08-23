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
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Message;

namespace GreenEnergyHub.Charges.Application.ChargeLinks
{
    public class ChargeLinkCommandHandler : IChargeLinkCommandHandler
    {
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;

        public ChargeLinkCommandHandler(IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
        }

        public async Task<ChargeLinksMessageResult> HandleAsync([NotNull]ChargeLinkCommandReceivedEvent chargeLinkCommand)
        {
            await _messageDispatcher.DispatchAsync(chargeLinkCommand).ConfigureAwait(false);

            var chargeLinksMessageResult = ChargeLinksMessageResult.CreateSuccess();
            chargeLinksMessageResult.CorrelationId = chargeLinkCommand.CorrelationId;

            return chargeLinksMessageResult;
        }
    }
}
