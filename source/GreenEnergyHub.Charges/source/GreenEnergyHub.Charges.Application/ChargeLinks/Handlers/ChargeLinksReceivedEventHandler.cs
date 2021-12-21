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
using GreenEnergyHub.Charges.Application.ChargeLinks.Acknowledgement;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksReceivedEventHandler : IChargeLinksReceivedEventHandler
    {
        private readonly IChargeLinksConfirmationService _chargeLinksConfirmationService;
        private readonly IChargeLinkFactory _chargeLinkFactory;
        private readonly IChargeLinkRepository _chargeLinkRepository;
        private readonly IChargeLinksCommandValidator _chargeLinksCommandValidator;

        public ChargeLinksReceivedEventHandler(
            IChargeLinksConfirmationService chargeLinksConfirmationService,
            IChargeLinkFactory chargeLinkFactory,
            IChargeLinkRepository chargeLinkRepository,
            IChargeLinksCommandValidator chargeLinksCommandValidator)
        {
            _chargeLinksConfirmationService = chargeLinksConfirmationService;
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
                await _chargeLinksConfirmationService.RejectAsync(chargeLinksReceivedEvent.ChargeLinksCommand, validationResult);
                return;
            }

            var chargeLinks = await _chargeLinkFactory.CreateAsync(chargeLinksReceivedEvent).ConfigureAwait(false);
            await _chargeLinkRepository.StoreAsync(chargeLinks).ConfigureAwait(false);
            await _chargeLinksConfirmationService.AcceptAsync(chargeLinksReceivedEvent.ChargeLinksCommand);
        }
    }
}
