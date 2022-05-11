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
using GreenEnergyHub.Charges.Application.ChargeCommands.Acknowledgement;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.ChargePrices.Handlers
{
    public class ChargePricesHandler : IChargePricesHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IInputValidator<ChargeOperationDto> _inputValidator;
        private readonly IBusinessValidator<ChargeOperationDto> _businessValidator;
        private readonly IChargeRepository _chargeInformationRepository;
        private readonly IChargePriceRepository _chargePriceRepository;
        private readonly IChargePriceFactory _chargePriceFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargePricesHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IInputValidator<ChargeOperationDto> inputValidator,
            IBusinessValidator<ChargeOperationDto> businessValidator,
            IChargeRepository chargeInformationRepository,
            IChargePriceRepository chargePriceRepository,
            IChargePriceFactory chargePriceFactory,
            IUnitOfWork unitOfWork)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _chargeInformationRepository = chargeInformationRepository;
            _chargePriceRepository = chargePriceRepository;
            _chargePriceFactory = chargePriceFactory;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(
            ChargeCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operationsToBeRejected = new List<ChargeOperationDto>();
            var rejectionRules = new List<IValidationRule>();
            var operationsToBeConfirmed = new List<ChargeOperationDto>();

            var operations = commandReceivedEvent.Command.ChargeOperations.ToArray();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];
                var chargeInformation = await GetChargeAsync(operation).ConfigureAwait(false);
                ArgumentNullException.ThrowIfNull(chargeInformation);

                var validationResult = _inputValidator.Validate(operation);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(toBeRejected => new PreviousOperationsMustBeValidRule(operation.Id, toBeRejected)));
                    break;
                }

                validationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(toBeRejected => new PreviousOperationsMustBeValidRule(operation.Id, toBeRejected)));
                    break;
                }

                await HandlePricesFromChargePriceOperationAsync(chargeInformation.Id, operation)
                    .ConfigureAwait(false);

                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            var document = commandReceivedEvent.Command.Document;
            await RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private async Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<ChargeOperationDto> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRule> rejectionRules)
        {
            if (operationsToBeRejected.Any())
            {
                await _chargeCommandReceiptService.RejectAsync(
                        new ChargeCommand(document, operationsToBeRejected),
                        ValidationResult.CreateFailure(rejectionRules))
                    .ConfigureAwait(false);
            }
        }

        private async Task AcceptValidOperationsAsync(
            IReadOnlyCollection<ChargeOperationDto> operationsToBeConfirmed,
            DocumentDto document)
        {
            if (operationsToBeConfirmed.Any())
            {
                await _chargeCommandReceiptService.AcceptAsync(
                    new ChargeCommand(document, operationsToBeConfirmed)).ConfigureAwait(false);
            }
        }

        private async Task HandlePricesFromChargePriceOperationAsync(
            Guid chargeInformationId,
            ChargeOperationDto operation)
        {
            var chargePrices = await GetChargePriceAsync(chargeInformationId, operation).ConfigureAwait(false);
            _chargePriceRepository.RemoveRange(chargePrices);
            foreach (var point in operation.Points)
            {
                var chargePrice = await _chargePriceFactory.CreateFromChargeOperationDtoAsync(operation, point)
                    .ConfigureAwait(false);
                await _chargePriceRepository.AddAsync(chargePrice).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<ChargePrice>> GetChargePriceAsync(Guid chargeId, ChargeOperationDto operationDto)
        {
            var orderedPoints = operationDto.Points.OrderBy(x => x.Time).ToList();
            ArgumentNullException.ThrowIfNull(orderedPoints);
            var startDate = orderedPoints.First().Time;
            var endDate = orderedPoints.Last().Time;
            return
                await _chargePriceRepository
                    .GetOrNullAsync(
                        chargeId,
                        startDate,
                        endDate).ConfigureAwait(false);
        }

        private async Task<Domain.ChargeInformation.ChargeInformation?> GetChargeAsync(ChargeOperationDto chargeOperationDto)
        {
            var chargeIdentifier = new ChargeInformationIdentifier(
                chargeOperationDto.ChargeId,
                chargeOperationDto.ChargeOwner,
                chargeOperationDto.Type);
            return await _chargeInformationRepository.GetOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
