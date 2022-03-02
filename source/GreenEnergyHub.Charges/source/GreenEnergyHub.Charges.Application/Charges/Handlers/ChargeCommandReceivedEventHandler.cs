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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Persistence;

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

            foreach (var chargeDto in commandReceivedEvent.Command.Charges)
            {
                var charge = await GetChargeAsync(chargeDto).ConfigureAwait(false);
                var operationType = GetOperationType(chargeDto, charge);

                switch (operationType)
                {
                    case OperationType.Create:
                        await HandleCreateEventAsync(chargeDto).ConfigureAwait(false);
                        break;
                    case OperationType.Update:
                        HandleUpdateEvent(charge, chargeDto);
                        break;
                    case OperationType.Stop:
                        StopCharge(charge, chargeDto);
                        break;
                    default:
                        throw new InvalidOperationException("Could not handle charge dto");
                }
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
        }

        private async Task HandleCreateEventAsync(ChargeDto chargeDto)
        {
            var charge = await _chargeFactory
                .CreateFromChargeOperationDtoAsync(chargeDto.ChargeOperation)
                .ConfigureAwait(false);

            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateEvent(Charge? charge, ChargeDto chargeCommand)
        {
            if (charge == null)
                throw new InvalidOperationException("Could not update charge. Charge not found.");

            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeCommand.ChargeOperation);
            charge.UpdateCharge(newChargePeriod);
            _chargeRepository.Update(charge);
        }

        private void StopCharge(Charge? charge, ChargeDto chargeDto)
        {
            if (charge == null)
                throw new InvalidOperationException("Could not stop charge. Charge not found.");

            if (chargeDto.ChargeOperation.EndDateTime == null)
                throw new InvalidOperationException("Could not stop charge. Invalid end date.");

            var chargeOperationEndDateTime = chargeDto.ChargeOperation.EndDateTime.Value;

            charge.StopCharge(chargeOperationEndDateTime);
            _chargeRepository.Update(charge);
        }

        private static OperationType GetOperationType(
            ChargeDto command,
            Charge? charge)
        {
            if (command.ChargeOperation.StartDateTime == command.ChargeOperation.EndDateTime)
            {
                return OperationType.Stop;
            }

            // Todo: If not first in list then it is also an update!
            return charge != null ? OperationType.Update : OperationType.Create;
        }

        private async Task<Charge?> GetChargeAsync(ChargeDto chargeDto)
        {
            var chargeIdentifier = new ChargeIdentifier(
                chargeDto.ChargeOperation.ChargeId,
                chargeDto.ChargeOperation.ChargeOwner,
                chargeDto.ChargeOperation.Type);

            return await _chargeRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
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
