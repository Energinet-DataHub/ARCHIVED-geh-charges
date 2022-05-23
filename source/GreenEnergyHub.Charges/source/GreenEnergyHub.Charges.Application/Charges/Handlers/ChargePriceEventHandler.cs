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
    public class ChargePriceEventHandler : IChargePriceEventHandler
    {
        private readonly IInputValidator<ChargeOperationDto> _inputValidator;
        private readonly IBusinessValidator<ChargeOperationDto> _businessValidator;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeCommandReceiptsForOperations _chargeCommandReceiptsForOperations;
        private readonly IUnitOfWork _unitOfWork;

        public ChargePriceEventHandler(
            IInputValidator<ChargeOperationDto> inputValidator,
            IBusinessValidator<ChargeOperationDto> businessValidator,
            IMarketParticipantRepository marketParticipantRepository,
            IChargeRepository chargeRepository,
            IChargeCommandReceiptsForOperations chargeCommandReceiptsForOperations,
            IUnitOfWork unitOfWork)
        {
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _marketParticipantRepository = marketParticipantRepository;
            _chargeRepository = chargeRepository;
            _chargeCommandReceiptsForOperations = chargeCommandReceiptsForOperations;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operationsToBeRejected = new List<ChargeOperationDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargeOperationDto>();

            var operations = commandReceivedEvent.Command.ChargeOperations.ToArray();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];
                var charge = await GetChargeAsync(operation).ConfigureAwait(false);
                if (charge is null)
                {
                    throw new InvalidOperationException($"Chargeinformation ID '{operation.ChargeId}' does not exist.");
                }

                var validationResult = _inputValidator.Validate(operation);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(_ =>
                            new OperationValidationRuleContainer(
                                new PreviousOperationsMustBeValidRule(operation.Id), operation.Id)));
                    break;
                }

                validationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(_ =>
                            new OperationValidationRuleContainer(
                                new PreviousOperationsMustBeValidRule(operation.Id), operation.Id)));
                    break;
                }

                charge.UpdatePrices(operation.StartDateTime, operation.EndDateTime, operation.Points);
                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            var document = commandReceivedEvent.Command.Document;
            await _chargeCommandReceiptsForOperations.RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await _chargeCommandReceiptsForOperations.AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private async Task<Charge?> GetChargeAsync(ChargeOperationDto operation)
        {
            var marketParticipant = await _marketParticipantRepository
                .SingleAsync(operation.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                operation.ChargeId,
                marketParticipant.Id,
                operation.Type);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
