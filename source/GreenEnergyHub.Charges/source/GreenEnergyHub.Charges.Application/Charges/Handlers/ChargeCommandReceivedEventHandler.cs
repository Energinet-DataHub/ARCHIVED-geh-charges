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

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IDocumentValidator<ChargeCommand> _documentValidator;
        private readonly IInputValidator<ChargeOperationDto> _inputValidator;
        private readonly IBusinessValidator<ChargeOperationDto> _businessValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IDocumentValidator<ChargeCommand> documentValidator,
            IInputValidator<ChargeOperationDto> inputValidator,
            IBusinessValidator<ChargeOperationDto> businessValidator,
            IChargeRepository chargeRepository,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IUnitOfWork unitOfWork)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _documentValidator = documentValidator;
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _chargeRepository = chargeRepository;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operationsToBeRejected = new List<ChargeOperationDto>();
            var operationsToBeConfirmed = new List<ChargeOperationDto>();

            var documentValidationResult = await _documentValidator
                .ValidateAsync(commandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                operationsToBeRejected.AddRange(commandReceivedEvent.Command.ChargeOperations);
            }
            else
            {
                var operations = commandReceivedEvent.Command.ChargeOperations.ToArray();

                for (var i = 0; i < operations.Length; i++)
                {
                    var operation = operations[i];
                    var charge = await GetChargeAsync(operation).ConfigureAwait(false);

                    var inputValidationResult = _inputValidator.Validate(operation);
                    if (inputValidationResult.IsFailed)
                    {
                        // TODO: All rejections should point to operation failed validation
                        operationsToBeRejected = operations[i..].ToList();
                        break;
                    }

                    var businessValidationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                    if (inputValidationResult.IsFailed)
                    {
                        // TODO: All rejections should point to operation failed validation
                        operationsToBeRejected = operations[i..].ToList();
                        break;
                    }

                    var operationType = GetOperationType(operation, charge);
                    switch (operationType)
                    {
                        /*
                         * In order to fix issue 1276, the create operation is allowed to violate the unit of work pattern.
                         * This is done to ensure the next operations in the bundle will be processed correctly.
                         * Do note that the ChargeCommandReceivedEventHandler is currently being refactored. So this is
                         * considered a short-sighted solution.
                         */
                        case OperationType.Create:
                            await HandleCreateEventAsync(operation).ConfigureAwait(false);
                            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
                            break;
                        case OperationType.Update:
                            HandleUpdateEvent(charge!, operation);
                            break;
                        case OperationType.Stop:
                            charge!.Stop(operation.EndDateTime);
                            break;
                        case OperationType.CancelStop:
                            HandleCancelStopEvent(charge!, operation);
                            break;
                        default:
                            throw new InvalidOperationException("Could not handle charge command.");
                    }

                    operationsToBeConfirmed.Add(operation);
                }

                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }

            // TODO: Send rejections
            // TODO: Send confirmations

            /* TODO:
             *  - businessValidation (and add to reject and confirm lists)
             *  - move functionality from "switch (operationType)" to individual handlers?
             *  - reject all operations in operationsToBeRejected
             *  - test confirm all operations in operationsToBeConfirmed
             *  - unitOfWork.SaveChanges();
            */

            // TODO: Delete the following when refactoring is done
            // var acceptedOperations = new List<ChargeOperationDto>();
            //
            // var triggeredBy = string.Empty;
            //
            // foreach (var chargeOperationDto in commandReceivedEvent.Command.ChargeOperations)
            // {
            //     var charge = await GetChargeAsync(commandReceivedEvent).ConfigureAwait(false);
            //     var chargeCommandWithOperation = new ChargeCommand(
            //         commandReceivedEvent.Command.Document,
            //         new List<ChargeOperationDto> { chargeOperationDto });
            //     triggeredBy = await HandleInvalidBusinessRulesAsync(
            //         chargeCommandWithOperation,
            //         triggeredBy).ConfigureAwait(false);
            //     if (!string.IsNullOrEmpty(triggeredBy)) continue;
            //
            //     var operationType = GetOperationType(chargeOperationDto, charge);
            //
            //     switch (operationType)
            //     {
            //         /*
            //          * In order to fix issue 1276, the create operation is allowed to violate the unit of work pattern.
            //          * This is done to ensure the next operations in the bundle will be processed correctly.
            //          * Do note that the ChargeCommandReceivedEventHandler is currently being refactored. So this is
            //          * considered a short-sighted solution.
            //          */
            //         case OperationType.Create:
            //             await HandleCreateEventAsync(chargeOperationDto).ConfigureAwait(false);
            //             await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            //             break;
            //         case OperationType.Update:
            //             HandleUpdateEvent(charge!, chargeOperationDto);
            //             break;
            //         case OperationType.Stop:
            //             charge!.Stop(chargeOperationDto.EndDateTime);
            //             break;
            //         case OperationType.CancelStop:
            //             HandleCancelStopEvent(charge!, chargeOperationDto);
            //             break;
            //         default:
            //             throw new InvalidOperationException("Could not handle charge command.");
            //     }
            //
            //     acceptedOperations.Add(chargeOperationDto);
            // }
            //
            // var acceptedChargeCommand = new ChargeCommand(commandReceivedEvent.Command.Document, acceptedOperations);
            // await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            // await _chargeCommandReceiptService.AcceptAsync(acceptedChargeCommand).ConfigureAwait(false);
        }

        // private async Task<string> HandleInvalidBusinessRulesAsync(
        //     ChargeCommand chargeCommandWithOperation,
        //     string triggeredBy)
        // {
        //     if (string.IsNullOrEmpty(triggeredBy))
        //     {
        //         var businessValidationResult =
        //             await _businessValidator.ValidateAsync(chargeCommandWithOperation).ConfigureAwait(false);
        //         if (businessValidationResult.IsFailed)
        //         {
        //             // First error found in bundle, we reject with the original validation error
        //             triggeredBy = chargeCommandWithOperation.ChargeOperations.Single().Id;
        //             await _chargeCommandReceiptService
        //                 .RejectAsync(chargeCommandWithOperation, businessValidationResult)
        //                 .ConfigureAwait(false);
        //         }
        //     }
        //     else
        //     {
        //         // A previous error has occured, we reject all subsequent operations in bundle with special validation error
        //         var rejectionValidationResult = ValidationResult.CreateFailure(new List<IValidationRule>()
        //         {
        //             new PreviousOperationsMustBeValidRule(triggeredBy, chargeCommandWithOperation.ChargeOperations.Single()),
        //         });
        //         await _chargeCommandReceiptService
        //             .RejectAsync(chargeCommandWithOperation, rejectionValidationResult)
        //             .ConfigureAwait(false);
        //     }
        //
        //     return triggeredBy;
        // }
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

        private void HandleCancelStopEvent(Charge charge, ChargeOperationDto chargeOperationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeOperationDto);
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

        private async Task<Charge?> GetChargeAsync(ChargeOperationDto chargeOperationDto)
        {
            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                chargeOperationDto.ChargeOwner,
                chargeOperationDto.Type);
            return await _chargeRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
