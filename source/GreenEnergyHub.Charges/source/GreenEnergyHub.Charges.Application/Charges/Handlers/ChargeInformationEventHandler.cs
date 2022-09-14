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
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeInformationEventHandler : IChargeInformationEventHandler
    {
        private readonly IInputValidator<ChargeInformationOperationDto> _inputValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChargeCommandReceiptService _chargeCommandReceiptService;

        public ChargeInformationEventHandler(
            IInputValidator<ChargeInformationOperationDto> inputValidator,
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IUnitOfWork unitOfWork,
            IChargeCommandReceiptService chargeCommandReceiptService)
        {
            _inputValidator = inputValidator;
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
            _unitOfWork = unitOfWork;
            _chargeCommandReceiptService = chargeCommandReceiptService;
        }

        public async Task HandleAsync(ChargeInformationCommandReceivedEvent chargeInformationCommandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargeInformationCommandReceivedEvent);

            var operations = chargeInformationCommandReceivedEvent.Command.Operations.ToArray();
            var document = chargeInformationCommandReceivedEvent.Command.Document;
            var operationsToBeRejected = new List<ChargeInformationOperationDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargeInformationOperationDto>();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];

                var inputValidationResult = _inputValidator.Validate(operation, document);
                if (inputValidationResult.IsFailed)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(rejectionRules, inputValidationResult, operationsToBeRejected, operation);
                    break;
                }

                var charge = await GetChargeAsync(operation).ConfigureAwait(false);

                try
                {
                    await HandleOperationAsync(operation, charge).ConfigureAwait(false);
                }
                catch (ChargeOperationFailedException exception)
                {
                    operationsToBeRejected = operations[i..].ToList();
                    CollectRejectionRules(
                        rejectionRules,
                        ValidationResult.CreateFailure(exception.InvalidRules),
                        operationsToBeRejected,
                        operation);
                    break;
                }

                operationsToBeConfirmed.Add(operation);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await _chargeCommandReceiptService.RejectInvalidOperationsAsync(operationsToBeRejected, document, rejectionRules).ConfigureAwait(false);
            await _chargeCommandReceiptService.AcceptValidOperationsAsync(operationsToBeConfirmed, document).ConfigureAwait(false);
        }

        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<ChargeInformationOperationDto> operationsToBeRejected,
            ChargeInformationOperationDto informationOperation)
        {
            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(subsequentOperation =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(informationOperation), subsequentOperation.OperationId)));
        }

        private async Task HandleOperationAsync(ChargeInformationOperationDto informationOperation, Charge? charge)
        {
            var operationType = GetOperationType(informationOperation, charge);
            switch (operationType)
            {
                case OperationType.Create:
                    await HandleCreateEventAsync(informationOperation).ConfigureAwait(false);
                    break;
                case OperationType.Update:
                    HandleUpdateEvent(charge!, informationOperation);
                    break;
                case OperationType.Stop:
                    charge!.Stop(informationOperation.EndDateTime);
                    break;
                case OperationType.CancelStop:
                    HandleCancelStopEvent(charge!, informationOperation);
                    break;
                default:
                    throw new InvalidOperationException("Could not handle charge command.");
            }
        }

        private async Task HandleCreateEventAsync(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var charge = await _chargeFactory
                .CreateFromChargeOperationDtoAsync(chargeInformationOperationDto)
                .ConfigureAwait(false);
            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateEvent(Charge charge, ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeInformationOperationDto);
            charge.Update(
                newChargePeriod,
                chargeInformationOperationDto.TaxIndicator,
                chargeInformationOperationDto.Resolution,
                chargeInformationOperationDto.OperationId);
        }

        private void HandleCancelStopEvent(Charge charge, ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeInformationOperationDto);
            charge.CancelStop(
                newChargePeriod,
                chargeInformationOperationDto.TaxIndicator,
                chargeInformationOperationDto.Resolution,
                chargeInformationOperationDto.OperationId);
        }

        private static OperationType GetOperationType(ChargeInformationOperationDto chargeInformationOperationDto, Charge? charge)
        {
            if (charge == null)
            {
                return OperationType.Create;
            }

            if (chargeInformationOperationDto.StartDateTime == chargeInformationOperationDto.EndDateTime)
            {
                return OperationType.Stop;
            }

            var latestChargePeriod = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            return chargeInformationOperationDto.StartDateTime == latestChargePeriod.EndDateTime
                ? OperationType.CancelStop
                : OperationType.Update;
        }

        private async Task<Charge?> GetChargeAsync(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var marketParticipant = await _marketParticipantRepository
                .GetSystemOperatorOrGridAccessProviderAsync(chargeInformationOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                chargeInformationOperationDto.SenderProvidedChargeId,
                marketParticipant.Id,
                chargeInformationOperationDto.ChargeType);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
