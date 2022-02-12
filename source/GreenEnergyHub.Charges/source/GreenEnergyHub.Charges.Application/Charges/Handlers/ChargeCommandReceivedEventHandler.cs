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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IValidator<ChargeCommand> _validator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IValidator<ChargeCommand> validator,
            IChargeRepository chargeRepository,
            IChargeFactory chargeFactory)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _validator = validator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            if (commandReceivedEvent == null) throw new ArgumentNullException(nameof(commandReceivedEvent));

            // input validation
            var inputValidationResult = _validator.InputValidate(commandReceivedEvent.Command);
            if (inputValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService
                    .RejectAsync(commandReceivedEvent.Command, inputValidationResult).ConfigureAwait(false);
                return;
            }

            // business validation
            var businessValidationResult = await _validator
                .BusinessValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (businessValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService.RejectAsync(
                    commandReceivedEvent.Command, businessValidationResult).ConfigureAwait(false);
                return;
            }

            // get charges
            var charges = await GetChargesAsync(commandReceivedEvent.Command).ConfigureAwait(false);

            // is create, update or stop?
            var operationType = GetOperationType(commandReceivedEvent.Command, charges);

            // create flow
            if (operationType == OperationType.Create)
            {
                var charge = await _chargeFactory
                    .CreateFromCommandAsync(commandReceivedEvent.Command)
                    .ConfigureAwait(false);
                await _chargeRepository.StoreChargeAsync(charge).ConfigureAwait(false);
            }

            // update flow
            if (operationType == OperationType.Update)
            {
                // new update flow
            }

            // stop flow
            if (operationType == OperationType.Update)
            {
                // new stop flow
            }

            await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
        }

        private static OperationType GetOperationType(ChargeCommand command, IEnumerable<Charge> charges)
        {
            if (command.ChargeOperation.StartDateTime == command.ChargeOperation.EndDateTime)
            {
                return OperationType.Stop;
            }

            return charges.Any() ? OperationType.Update : OperationType.Create;
        }

        private async Task<List<Charge>> GetChargesAsync(ChargeCommand command)
        {
            var chargeIdentifier = new ChargeIdentifier(
                command.ChargeOperation.ChargeId,
                command.ChargeOperation.ChargeOwner,
                command.ChargeOperation.Type);

            return await _chargeRepository.GetChargesAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }

    // Internal, so far...
    internal enum OperationType
    {
        Create = 0,
        Update = 1,
        Stop = 2,
    }
}
