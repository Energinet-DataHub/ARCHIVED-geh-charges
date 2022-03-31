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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IValidator<ChargeCommand, ChargeOperationComposite> _validator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IValidator<ChargeCommand, ChargeOperationComposite> validator,
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

            var triggeredBy = string.Empty;
            var charge = await GetChargeAsync(commandReceivedEvent).ConfigureAwait(false);
            foreach (var chargeOperationDto in commandReceivedEvent.Command.Charges)
            {
                triggeredBy = await HandleInvalidBusinessRulesAsync(
                    commandReceivedEvent,
                    chargeOperationDto,
                    triggeredBy).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(triggeredBy)) continue;

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
                        HandleCancelStopEvent(charge!, commandReceivedEvent.Command);                        
                        break;
                    default:
                        throw new InvalidOperationException("Could not handle charge command.");
                }

                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                await _chargeCommandReceiptService.AcceptAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            }
        }

        private async Task<string> HandleInvalidBusinessRulesAsync(
            ChargeCommandReceivedEvent commandReceivedEvent,
            ChargeOperationDto chargeOperationDto,
            string triggeredBy)
        {
            switch (string.IsNullOrEmpty(triggeredBy))
            {
                case true:
                    var chargeOperationComposite =
                        new ChargeOperationComposite(commandReceivedEvent.Command, chargeOperationDto);
                    var businessValidationResult =
                        await _validator.BusinessValidateAsync(chargeOperationComposite).ConfigureAwait(false);
                    if (businessValidationResult.IsFailed)
                    {
                        // First error found in bundle, we reject with the original validation error
                        triggeredBy = chargeOperationDto.Id;
                        await _chargeCommandReceiptService
                            .RejectAsync(commandReceivedEvent.Command, businessValidationResult)
                            .ConfigureAwait(false);
                    }

                    break;
                case false:
                    // A previous error has occured, we reject all subsequent operations in bundle with special validation error
                    var rejectionValidationResult = ValidationResult.CreateFailure(new List<IValidationRule>()
                    {
                        new PreviousOperationRulesMustBeValid(triggeredBy, chargeOperationDto),
                    });
                    await _chargeCommandReceiptService
                        .RejectAsync(commandReceivedEvent.Command, rejectionValidationResult)
                        .ConfigureAwait(false);
                    break;
            }

            return triggeredBy;
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

        private void HandleCancelStopEvent(Charge charge, ChargeCommand chargeCommand)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeCommand.ChargeOperation);
            charge.CancelStop(newChargePeriod);
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
            return chargeOperationDto.StartDateTime == latestChargePeriod.EndDateTime
                ? OperationType.CancelStop
                : OperationType.Update;
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
