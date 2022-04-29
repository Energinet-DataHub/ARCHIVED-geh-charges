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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeAndPriceHandler : IChargeAndPriceHandler
    {
        private readonly IChargeCommandReceivedEventHandler _chargeCommandReceivedEventHandler;
        private readonly IMessageDispatcher<ChargeCommandAcceptedEvent> _messageDispatcher;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;

        public ChargeAndPriceHandler(
            IChargeCommandReceivedEventHandler chargeCommandReceivedEventHandler,
            IMessageDispatcher<ChargeCommandAcceptedEvent> messageDispatcher,
            IChargeCommandAcceptedEventFactory chargeCommandAcceptedEventFactory)
        {
            _chargeCommandReceivedEventHandler = chargeCommandReceivedEventHandler;
            _messageDispatcher = messageDispatcher;
            _chargeCommandAcceptedEventFactory = chargeCommandAcceptedEventFactory;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            switch (commandReceivedEvent.Command.Document.BusinessReasonCode)
            {
                case BusinessReasonCode.UpdatePriceInformation:
                    var chargeCommandAcceptedEvent =
                        _chargeCommandAcceptedEventFactory.CreateEvent(commandReceivedEvent.Command);
                    await _messageDispatcher.DispatchAsync(chargeCommandAcceptedEvent)
                        .ConfigureAwait(false);
                    break;
                case BusinessReasonCode.UpdateChargeInformation:
                    await _chargeCommandReceivedEventHandler.HandleAsync(commandReceivedEvent).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Invalid BusinessReasonCode {commandReceivedEvent.Command.Document.BusinessReasonCode}");
            }
        }
    }
}
