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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksReceivedEventHandler : IChargeLinksReceivedEventHandler
    {
        private readonly IMessageDispatcher<ChargeLinksAcceptedEvent> _messageDispatcher;
        private readonly IChargeLinksAcceptedEventFactory _chargeLinksAcceptedEventFactory;
        private readonly IChargeLinkFactory _chargeLinkFactory;
        private readonly IChargeLinkRepository _chargeLinkRepository;
        private readonly IChargeLinksCommandValidator _chargeLinksCommandValidator;

        public ChargeLinksReceivedEventHandler(
            IMessageDispatcher<ChargeLinksAcceptedEvent> messageDispatcher,
            IChargeLinksAcceptedEventFactory chargeLinksAcceptedEventFactory,
            IChargeLinkFactory chargeLinkFactory,
            IChargeLinkRepository chargeLinkRepository,
            IChargeLinksCommandValidator chargeLinksCommandValidator)
        {
            _messageDispatcher = messageDispatcher;
            _chargeLinksAcceptedEventFactory = chargeLinksAcceptedEventFactory;
            _chargeLinkFactory = chargeLinkFactory;
            _chargeLinkRepository = chargeLinkRepository;
            _chargeLinksCommandValidator = chargeLinksCommandValidator;
        }

        public async Task HandleAsync(ChargeLinksReceivedEvent chargeLinksReceivedEvent)
        {
            // Upcoming stories will cover the update scenarios where charge link already exists
            var validationResult = await _chargeLinksCommandValidator.ValidateAsync(chargeLinksReceivedEvent.ChargeLinksCommand).ConfigureAwait(false);
            if (validationResult.IsFailed)
            {
                // Create rejection events TODO await _chargeCommandConfirmationService.RejectAsync(chargeLinksReceivedEvent.ChargeLinksCommand, validationResult).ConfigureAwait(false);
                return;
            }

            var chargeLinks = await _chargeLinkFactory.CreateAsync(chargeLinksReceivedEvent).ConfigureAwait(false);
            await _chargeLinkRepository.StoreAsync(chargeLinks).ConfigureAwait(false);

            var chargeLinkCommandAcceptedEvent = _chargeLinksAcceptedEventFactory.Create(
                chargeLinksReceivedEvent.ChargeLinksCommand);

            await _messageDispatcher.DispatchAsync(chargeLinkCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
