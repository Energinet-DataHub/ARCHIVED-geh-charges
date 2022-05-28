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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;
        private readonly IInputValidator<ChargeOperationDto> _inputValidator;
        private readonly IBusinessValidator<ChargeOperationDto> _businessValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeCommandReceivedEventHandler(
            IChargeCommandReceiptService chargeCommandReceiptService,
            IInputValidator<ChargeOperationDto> inputValidator,
            IBusinessValidator<ChargeOperationDto> businessValidator,
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IUnitOfWork unitOfWork)
        {
            _chargeCommandReceiptService = chargeCommandReceiptService;
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
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

                var validationResult = _inputValidator.Validate(operation);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(o =>
                            new OperationValidationRuleContainer(
                                new PreviousOperationsMustBeValidRule(operation.Id), o.Id)));
                    break;
                }

                validationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
                if (validationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    rejectionRules.AddRange(validationResult.InvalidRules);
                    rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                        .Select(o =>
                            new OperationValidationRuleContainer(
                                new PreviousOperationsMustBeValidRule(operation.Id), o.Id)));
                    break;
                }

                var operationType = GetOperationType(operation, charge);
                switch (operationType)
                {
                    case OperationType.Create:
                        await HandleCreateEventAsync(operation).ConfigureAwait(false);
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

            var document = commandReceivedEvent.Command.Document;
            await RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private async Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<ChargeOperationDto> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRuleContainer> rejectionRules)
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
            var marketParticipant = await _marketParticipantRepository
                .SingleAsync(chargeOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                marketParticipant.Id,
                chargeOperationDto.Type);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
