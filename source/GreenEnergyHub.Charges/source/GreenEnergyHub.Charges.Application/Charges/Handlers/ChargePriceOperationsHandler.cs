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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargePriceOperationsHandler : IChargePriceOperationsHandler
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IInputValidator<ChargePriceOperationDto> _inputValidator;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly ILogger _logger;
        private readonly IChargePriceOperationsConfirmedEventFactory _chargePriceOperationsConfirmedEventFactory;
        private readonly IChargePriceOperationsRejectedEventFactory _chargePriceOperationsRejectedEventFactory;

        public ChargePriceOperationsHandler(
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IInputValidator<ChargePriceOperationDto> inputValidator,
            IDomainEventPublisher domainEventPublisher,
            ILoggerFactory loggerFactory,
            IChargePriceOperationsConfirmedEventFactory chargePriceOperationsConfirmedEventFactory,
            IChargePriceOperationsRejectedEventFactory chargePriceOperationsRejectedEventFactory)
        {
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _inputValidator = inputValidator;
            _domainEventPublisher = domainEventPublisher;
            _chargePriceOperationsConfirmedEventFactory = chargePriceOperationsConfirmedEventFactory;
            _chargePriceOperationsRejectedEventFactory = chargePriceOperationsRejectedEventFactory;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceOperationsHandler));
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent chargePriceCommandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(chargePriceCommandReceivedEvent);

            var operations = chargePriceCommandReceivedEvent.Command.Operations.ToArray();
            var document = chargePriceCommandReceivedEvent.Command.Document;
            var operationsToBeRejected = new List<ChargePriceOperationDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargePriceOperationDto>();

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
                    if (charge is null)
                    {
                        throw new InvalidOperationException($"Charge ID '{operation.SenderProvidedChargeId}' does not exist.");
                    }

                    charge.UpdatePrices(
                        operation.PointsStartInterval,
                        operation.PointsEndInterval,
                        operation.Points,
                        operation.OperationId);
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
            IReadOnlyCollection<ChargePriceOperationDto> operationsToBeConfirmed)
        {
            if (!operationsToBeConfirmed.Any()) return;
            RaiseConfirmedEvent(document, operationsToBeConfirmed);
        }

        private void HandleRejections(
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operationsToBeRejected,
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
            IReadOnlyCollection<ChargePriceOperationDto> operationsToBeConfirmed)
        {
            if (!operationsToBeConfirmed.Any()) return;
            var confirmedEvent = _chargePriceOperationsConfirmedEventFactory.Create(document, operationsToBeConfirmed);
            _domainEventPublisher.Publish(confirmedEvent);
        }

        private void RaiseRejectedEvent(
            DocumentDto document,
            IReadOnlyCollection<ChargePriceOperationDto> operationsToBeRejected,
            IList<IValidationRuleContainer> rejectionRules)
        {
            if (!operationsToBeRejected.Any()) return;
            var validationResult = ValidationResult.CreateFailure(rejectionRules);
            var rejectedEvent = _chargePriceOperationsRejectedEventFactory.Create(document, operationsToBeRejected, validationResult);
            _domainEventPublisher.Publish(rejectedEvent);
        }

        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<ChargePriceOperationDto> operationsToBeRejected,
            ChargePriceOperationDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(subsequentOperation =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(operation), subsequentOperation.OperationId)));
        }

        private async Task<Charge?> GetChargeAsync(ChargePriceOperationDto chargeOperationDto)
        {
            var marketParticipant = await _marketParticipantRepository
                .GetSystemOperatorOrGridAccessProviderAsync(chargeOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.SenderProvidedChargeId,
                marketParticipant.Id,
                chargeOperationDto.ChargeType);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
