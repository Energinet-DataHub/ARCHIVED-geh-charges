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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandConfirmationService _chargeCommandConfirmationService;
        private readonly IChargeCommandValidator _chargeCommandValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargeCommandFactory _chargeCommandFactory;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandConfirmationService chargeCommandConfirmationService,
            IChargeCommandValidator chargeCommandValidator,
            IChargeRepository chargeRepository,
            IChargeFactory chargeFactory,
            IChargeCommandFactory chargeCommandFactory)
        {
            _chargeCommandConfirmationService = chargeCommandConfirmationService;
            _chargeCommandValidator = chargeCommandValidator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
            _chargeCommandFactory = chargeCommandFactory;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            if (commandReceivedEvent == null) throw new ArgumentNullException(nameof(commandReceivedEvent));

            var validationResult = await _chargeCommandValidator.ValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (validationResult.IsFailed)
            {
                await _chargeCommandConfirmationService.RejectAsync(commandReceivedEvent.Command, validationResult).ConfigureAwait(false);
                return;
            }

            var charge = await _chargeFactory.CreateFromCommandAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            await _chargeRepository.StoreChargeAsync(
                    charge,
                    commandReceivedEvent.Command.Document.Sender.Id,
                    commandReceivedEvent.Command.Document.RequestDate)
                .ConfigureAwait(false);

            var chargeCommand = _chargeCommandFactory.CreateFromCharge(charge, commandReceivedEvent.Command.Document);
            await _chargeCommandConfirmationService.AcceptAsync(chargeCommand).ConfigureAwait(false);
        }
    }
}
