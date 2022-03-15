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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Persistence;
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
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IValidator<ChargeCommand> validator,
            IChargeRepository chargeRepository,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IUnitOfWork unitOfWork)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _validator = validator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            if (commandReceivedEvent == null) throw new ArgumentNullException(nameof(commandReceivedEvent));

            var inputValidationResult = _validator.InputValidate(commandReceivedEvent.Command);
            if (inputValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService
                    .RejectAsync(commandReceivedEvent.Command, inputValidationResult).ConfigureAwait(false);
                return;
            }

            var businessValidationResult = await _validator
                .BusinessValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (businessValidationResult.IsFailed)
            {
                await _chargeCommandReceiptService.RejectAsync(
                    commandReceivedEvent.Command, businessValidationResult).ConfigureAwait(false);
                return;
            }

            var charge = await GetChargeAsync(commandReceivedEvent.Command).ConfigureAwait(false);

            var operationType = GetOperationType(commandReceivedEvent.Command, charge);

            switch (operationType)
            {
                case OperationType.Create:
                    await HandleCreateEventAsync(commandReceivedEvent.Command).ConfigureAwait(false);
                    break;
                case OperationType.Update:
                    HandleUpdateEvent(charge!, commandReceivedEvent.Command);
                    break;
                case OperationType.Stop:
                    charge!.Stop(commandReceivedEvent.Command.ChargeOperation.EndDateTime);
                    break;
                case OperationType.CancelStop:
                    charge!.CancelStop();
                    break;
                default:
                    throw new InvalidOperationException("Could not handle charge command.");
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
        }

        private async Task HandleCreateEventAsync(ChargeCommand chargeCommand)
        {
            var charge = await _chargeFactory
                .CreateFromCommandAsync(chargeCommand)
                .ConfigureAwait(false);

            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateEvent(Charge charge, ChargeCommand chargeCommand)
        {
            var newChargePeriod = _chargePeriodFactory.CreateUpdateFromChargeOperationDto(chargeCommand.ChargeOperation);
            charge.Update(newChargePeriod);
        }

        private static OperationType GetOperationType(ChargeCommand command, Charge? charge)
        {
            if (charge == null)
            {
                return OperationType.Create;
            }

            if (command.ChargeOperation.StartDateTime == command.ChargeOperation.EndDateTime)
            {
                return OperationType.Stop;
            }

            var latestChargePeriod = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            var operationType =
                command.ChargeOperation.StartDateTime == latestChargePeriod.StartDateTime && latestChargePeriod.IsStop ?
                OperationType.CancelStop : OperationType.Update;

            return operationType;
        }

        private async Task<Charge?> GetChargeAsync(ChargeCommand command)
        {
            var chargeIdentifier = new ChargeIdentifier(
                command.ChargeOperation.ChargeId,
                command.ChargeOperation.ChargeOwner,
                command.ChargeOperation.Type);
            return await _chargeRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
