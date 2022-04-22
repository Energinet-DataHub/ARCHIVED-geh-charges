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

using System;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.PriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandHandler : IChargeCommandHandler
    {
        private readonly IClock _clock;
        private readonly IMessageDispatcher<PriceCommandReceivedEvent> _priceMessageDispatcher;
        private readonly IMessageDispatcher<ChargeCommandReceivedEvent> _chargeMessageDispatcher;

        public ChargeCommandHandler(
            IClock clock,
            IMessageDispatcher<PriceCommandReceivedEvent> priceMessageDispatcher,
            IMessageDispatcher<ChargeCommandReceivedEvent> chargeMessageDispatcher)
        {
            _clock = clock;
            _priceMessageDispatcher = priceMessageDispatcher;
            _chargeMessageDispatcher = chargeMessageDispatcher;
        }

        public async Task HandleAsync(ChargeCommand command)
        {
            switch (command.Document.BusinessReasonCode)
            {
                case BusinessReasonCode.UpdatePriceInformation:
                    await DispatchPriceCommandReceivedEventAsync(command).ConfigureAwait(false);
                    break;
                case BusinessReasonCode.UpdateChargeInformation:
                    await DispatchChargeCommandReceivedEventAsync(command).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid BusinessReasonCode {command.Document.BusinessReasonCode}");
            }
        }

        private async Task DispatchPriceCommandReceivedEventAsync(ChargeCommand command)
        {
            var receivedEvent = new PriceCommandReceivedEvent(_clock.GetCurrentInstant(), command);
            await _priceMessageDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);
        }

        private async Task DispatchChargeCommandReceivedEventAsync(ChargeCommand command)
        {
            var receivedEvent = new ChargeCommandReceivedEvent(_clock.GetCurrentInstant(), command);
            await _chargeMessageDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);
        }
    }
}
