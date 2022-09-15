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
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Common.Helpers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeInformationOperationsHandler : IChargeInformationOperationsHandler
    {
        private readonly IInputValidator<ChargeInformationOperationDto> _inputValidator;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeFactory _chargeFactory;
        private readonly IChargePeriodFactory _chargePeriodFactory;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IChargeInformationOperationsAcceptedEventFactory _chargeInformationOperationsAcceptedEventFactory;
        private readonly IChargeInformationOperationsRejectedEventFactory _chargeInformationOperationsRejectedEventFactory;
        private readonly ILogger _logger;

        public ChargeInformationOperationsHandler(
            IInputValidator<ChargeInformationOperationDto> inputValidator,
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IChargeFactory chargeFactory,
            IChargePeriodFactory chargePeriodFactory,
            IDomainEventPublisher domainEventPublisher,
            ILoggerFactory loggerFactory,
            IChargeInformationOperationsAcceptedEventFactory chargeInformationOperationsAcceptedEventFactory,
            IChargeInformationOperationsRejectedEventFactory chargeInformationOperationsRejectedEventFactory)
        {
            _inputValidator = inputValidator;
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _chargeFactory = chargeFactory;
            _chargePeriodFactory = chargePeriodFactory;
            _domainEventPublisher = domainEventPublisher;
            _chargeInformationOperationsAcceptedEventFactory = chargeInformationOperationsAcceptedEventFactory;
            _chargeInformationOperationsRejectedEventFactory = chargeInformationOperationsRejectedEventFactory;
            _logger = loggerFactory.CreateLogger(nameof(ChargeInformationOperationsHandler));
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

            HandleConfirmations(document, operationsToBeConfirmed);
            HandleRejections(document, operationsToBeRejected, rejectionRules);
        }

        private void HandleConfirmations(
            DocumentDto document,
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeConfirmed)
        {
            if (!operationsToBeConfirmed.Any()) return;
            RaiseConfirmedEvent(document, operationsToBeConfirmed);
        }

        private void HandleRejections(
            DocumentDto document,
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeRejected,
            IList<IValidationRuleContainer> rejectionRules)
        {
            ArgumentNullException.ThrowIfNull(operationsToBeRejected);
            ArgumentNullException.ThrowIfNull(rejectionRules);

            if (!operationsToBeRejected.Any()) return;

            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(document, rejectionRules);
            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);

            RaiseRejectedEvent(document, operationsToBeRejected, rejectionRules);
        }

        private void RaiseConfirmedEvent(
            DocumentDto document,
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeConfirmed)
        {
            if (!operationsToBeConfirmed.Any()) return;
            var confirmedEvent = _chargeInformationOperationsAcceptedEventFactory.Create(document, operationsToBeConfirmed);
            _domainEventPublisher.Publish(confirmedEvent);
        }

        private void RaiseRejectedEvent(
            DocumentDto document,
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeRejected,
            IList<IValidationRuleContainer> rejectionRules)
        {
            if (!operationsToBeRejected.Any()) return;
            var validationResult = ValidationResult.CreateFailure(rejectionRules);
            var rejectedEvent = _chargeInformationOperationsRejectedEventFactory.Create(document, operationsToBeRejected, validationResult);
            _domainEventPublisher.Publish(rejectedEvent);
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
                    await HandleCreateOperationAsync(informationOperation).ConfigureAwait(false);
                    break;
                case OperationType.Update:
                    HandleUpdateOperationEvent(charge!, informationOperation);
                    break;
                case OperationType.Stop:
                    charge!.Stop(informationOperation.EndDateTime);
                    break;
                case OperationType.CancelStop:
                    HandleCancelStopOperationEvent(charge!, informationOperation);
                    break;
                default:
                    throw new InvalidOperationException("Could not handle charge command.");
            }
        }

        private async Task HandleCreateOperationAsync(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var charge = await _chargeFactory
                .CreateFromChargeOperationDtoAsync(chargeInformationOperationDto)
                .ConfigureAwait(false);
            await _chargeRepository.AddAsync(charge).ConfigureAwait(false);
        }

        private void HandleUpdateOperationEvent(Charge charge, ChargeInformationOperationDto chargeInformationOperationDto)
        {
            var newChargePeriod = _chargePeriodFactory.CreateFromChargeOperationDto(chargeInformationOperationDto);
            charge.Update(
                newChargePeriod,
                chargeInformationOperationDto.TaxIndicator,
                chargeInformationOperationDto.Resolution,
                chargeInformationOperationDto.OperationId);
        }

        private void HandleCancelStopOperationEvent(Charge charge, ChargeInformationOperationDto chargeInformationOperationDto)
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
