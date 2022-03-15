﻿// Copyright 2020 Energinet DataHub A/S
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

            // Todo: Add rejected operations to a list to that can be handled/rejected later
            // All operations after the first rejected should also be rejected
            var inputValidationResult = _validator.InputValidate(commandReceivedEvent.Command);
            if (inputValidationResult.IsFailed)
            {
                // Todo: Put rejected operations in a list to be handled/rejected later
                await _chargeCommandReceiptService
                    .RejectAsync(commandReceivedEvent.Command, inputValidationResult).ConfigureAwait(false);
                return;
            }

            var businessValidationResult = await _validator
                .BusinessValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (businessValidationResult.IsFailed)
            {
                // Todo: Add rejected operations to a list that can be handled/rejected later
                await _chargeCommandReceiptService.RejectAsync(
                    commandReceivedEvent.Command, businessValidationResult).ConfigureAwait(false);
                return;
            }

            var charge = await GetChargeAsync(commandReceivedEvent).ConfigureAwait(false);

            // Todo: Only handle those that were not rejected!
            foreach (var chargeOperationDto in commandReceivedEvent.Command.Charges)
            {
                var operationType = GetOperationType(chargeOperationDto, charge);

                switch (operationType)
                {
                    case OperationType.Create:
                        await HandleCreateEventAsync(chargeOperationDto).ConfigureAwait(false);
                        break;
                    case OperationType.Update:
                        HandleUpdateEvent(charge!, chargeOperationDto);
                        break;
                    case OperationType.Stop:
                        charge!.Stop(chargeOperationDto.EndDateTime);
                        break;
                    case OperationType.CancelStop:
                        charge!.CancelStop();
                        break;
                    default:
                        throw new InvalidOperationException("Could not handle charge command.");
                }

                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }

            // Todo: Change to a accept list of operations? - and only for those not rejected
            await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
        }

        private async Task HandleCreateEventAsync(ChargeOperationDto chargeOperationDto)
        {
            var charge = await _chargeFactory
                .CreateFromChargeOperationDtoAsync(chargeOperationDto)
                .ConfigureAwait(false);

            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateEvent(Charge charge, ChargeOperationDto chargeOperationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeOperationDto);
            charge.Update(newChargePeriod);
        }

        private static OperationType GetOperationType(ChargeOperationDto chargeOperationDto, Charge? charge)
        {
            if (charge == null)
            {
                return OperationType.Create;
            }

            if (chargeOperationDto.StartDateTime == chargeOperationDto.EndDateTime)
            {
                return OperationType.Stop;
            }

            var latestChargePeriod = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            return chargeOperationDto.StartDateTime == latestChargePeriod.EndDateTime ?
                OperationType.CancelStop : OperationType.Update;
        }

        private async Task<Charge?> GetChargeAsync(ChargeCommandReceivedEvent chargeCommandReceivedEvent)
        {
            var chargeOperationDto = chargeCommandReceivedEvent.Command.Charges.First();

            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                chargeOperationDto.ChargeOwner,
                chargeOperationDto.Type);
            return await _chargeRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
