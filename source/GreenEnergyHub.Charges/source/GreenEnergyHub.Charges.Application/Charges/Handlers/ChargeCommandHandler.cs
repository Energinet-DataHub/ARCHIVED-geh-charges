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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandHandler : IChargeCommandHandler
    {
        private readonly IClock _clock;
        private readonly IMessageDispatcher<ChargeCommandReceivedEvent> _chargeMessageDispatcher;
        private readonly IMessageDispatcher<ChargePriceCommandReceivedEvent> _chargePriceMessageDispatcher;

        public ChargeCommandHandler(
            IClock clock,
            IMessageDispatcher<ChargeCommandReceivedEvent> chargeMessageDispatcher,
            IMessageDispatcher<ChargePriceCommandReceivedEvent> chargePriceMessageDispatcher)
        {
            _clock = clock;
            _chargeMessageDispatcher = chargeMessageDispatcher;
            _chargePriceMessageDispatcher = chargePriceMessageDispatcher;
        }

        public async Task HandleAsync(ChargeCommand command)
        {
            var receivedEvent = new ChargeCommandReceivedEvent(_clock.GetCurrentInstant(), command);
            await _chargeMessageDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);

            // TODO: In step with the price flow expansion taking place this should be replaced by code that
            // differentiates between ChargeInformation and ChargePrice commands
            if (command.Document.BusinessReasonCode == BusinessReasonCode.UpdateChargePrices)
            {
                var priceReceivedEvent = new ChargePriceCommandReceivedEvent(_clock.GetCurrentInstant(), command);
                await _chargePriceMessageDispatcher.DispatchAsync(priceReceivedEvent).ConfigureAwait(false);
            }
        }
    }
}
