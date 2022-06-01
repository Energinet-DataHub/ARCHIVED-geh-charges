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
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeInformationEventHandler : IChargeInformationEventHandler
    {
        private readonly IInputValidator<ChargeInformationDto> _inputValidator;
        private readonly IBusinessValidator<ChargeOperation> _businessValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargeInformationEventHandler(
            IInputValidator<ChargeInformationDto> inputValidator,
            IBusinessValidator<ChargeOperation> businessValidator,
            IChargeRepository chargeRepository,
            IChargeIdentifierFactory chargeIdentifierFactory,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IUnitOfWork unitOfWork,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _chargeRepository = chargeRepository;
            _chargeIdentifierFactory = chargeIdentifierFactory;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
            _unitOfWork = unitOfWork;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operations = commandReceivedEvent.Command.ChargeOperations.ToArray();
            var operationsToBeRejected = new List<ChargeOperation>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargeOperation>();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];
                var charge = await GetChargeAsync(operation).ConfigureAwait(false);

                var validationResult = _inputValidator.Validate((ChargeInformationDto)operation);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, validationResult, operationsToBeRejected, operation);
                    break;
                }

                validationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, validationResult, operationsToBeRejected, operation);
                    break;
                }

                await HandleOperationAsync(operation, charge).ConfigureAwait(false);
                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            var document = commandReceivedEvent.Command.Document;
            await _chargeCommandReceiptService.RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await _chargeCommandReceiptService.AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<ChargeOperation> operationsToBeRejected,
            ChargeOperation operation)
        {
            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(subsequentOperation =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(operation.Id), subsequentOperation.Id)));
        }

        private async Task HandleOperationAsync(ChargeOperation operation, Charge? charge)
        {
            var informationDto = (ChargeInformationDto)operation;
            var operationType = GetOperationType(informationDto, charge);

            switch (operationType)
            {
                case OperationType.Create:
                    await HandleCreateEventAsync(informationDto).ConfigureAwait(false);
                    break;
                case OperationType.Update:
                    HandleUpdateEvent(charge!, informationDto);
                    break;
                case OperationType.Stop:
                    charge!.Stop(informationDto.EndDateTime);
                    break;
                case OperationType.CancelStop:
                    HandleCancelStopEvent(charge!, informationDto);
                    break;
                default:
                    throw new InvalidOperationException("Could not handle charge command.");
            }
        }

        private async Task HandleCreateEventAsync(ChargeInformationDto chargeInformationDto)
        {
            var charge = await _chargeFactory
                .CreateFromChargeOperationDtoAsync(chargeInformationDto)
                .ConfigureAwait(false);

            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateEvent(Charge charge, ChargeInformationDto chargeInformationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeInformationDto);
            charge.Update(newChargePeriod);
        }

        private void HandleCancelStopEvent(Charge charge, ChargeInformationDto chargeInformationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeInformationDto);
            charge.CancelStop(newChargePeriod);
        }

        private static OperationType GetOperationType(ChargeInformationDto chargeInformationDto, Charge? charge)
        {
            if (charge == null)
            {
                return OperationType.Create;
            }

            if (chargeInformationDto.StartDateTime == chargeInformationDto.EndDateTime)
            {
                return OperationType.Stop;
            }

            var latestChargePeriod = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            return chargeInformationDto.StartDateTime == latestChargePeriod.EndDateTime
                ? OperationType.CancelStop
                : OperationType.Update;
        }

        private async Task<Charge?> GetChargeAsync(ChargeOperation chargeInformationDto)
        {
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(chargeInformationDto.ChargeId, chargeInformationDto.Type, chargeInformationDto.ChargeOwner)
                .ConfigureAwait(false);

            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
