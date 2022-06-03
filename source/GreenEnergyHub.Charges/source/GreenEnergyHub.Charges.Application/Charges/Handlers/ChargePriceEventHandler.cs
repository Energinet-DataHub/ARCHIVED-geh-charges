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
    public class ChargePriceEventHandler : IChargePriceEventHandler
    {
        private readonly IInputValidator<ChargePriceDto> _inputValidator;
        private readonly IBusinessValidator<ChargePriceDto> _businessValidator;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IChargeRepository _chargeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargePriceEventHandler(
            IInputValidator<ChargePriceDto> inputValidator,
            IBusinessValidator<ChargePriceDto> businessValidator,
            IChargeIdentifierFactory chargeIdentifierFactory,
            IChargeRepository chargeRepository,
            IUnitOfWork unitOfWork,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _chargeIdentifierFactory = chargeIdentifierFactory;
            _chargeRepository = chargeRepository;
            _unitOfWork = unitOfWork;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operations = commandReceivedEvent.Command.ChargeOperations.ToArray();
            var operationsToBeRejected = new List<IChargeOperation>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<IChargeOperation>();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];
                var chargePriceDto = (ChargePriceDto)operation;
                var charge = await GetChargeAsync(operation).ConfigureAwait(false);
                if (charge is null)
                {
                    throw new InvalidOperationException($"Charge ID '{operation.ChargeId}' does not exist.");
                }

                var validationResult = _inputValidator.Validate(chargePriceDto);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, validationResult, operationsToBeRejected, operation);
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(_ =>
                            new OperationValidationRuleContainer(
                                new PreviousOperationsMustBeValidRule(operation.Id), operation.Id)));
                    break;
                }

                validationResult = await _businessValidator.ValidateAsync(chargePriceDto).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, validationResult, operationsToBeRejected, operation);
                    break;
                }

                HandleOperation(operation, charge);
                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            var document = commandReceivedEvent.Command.Document;
            await _chargeCommandReceiptService.RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await _chargeCommandReceiptService.AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private static void HandleOperation(IChargeOperation operation, Charge charge)
        {
            var priceDto = (ChargePriceDto)operation;
            charge.UpdatePrices(priceDto.PointsStartInterval, priceDto.PointsEndInterval, priceDto.Points);
        }

        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<IChargeOperation> operationsToBeRejected,
            IChargeOperation information)
        {
            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(_ =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(information.Id), information.Id)));
        }

        private async Task<Charge?> GetChargeAsync(IChargeOperation chargeOperation)
        {
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(chargeOperation.ChargeId, chargeOperation.Type, chargeOperation.ChargeOwner)
                .ConfigureAwait(false);

            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
