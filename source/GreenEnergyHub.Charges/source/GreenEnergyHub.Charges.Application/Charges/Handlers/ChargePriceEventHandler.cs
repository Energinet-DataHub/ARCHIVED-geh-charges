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
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargePriceEventHandler : IChargePriceEventHandler
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IInputValidator<ChargePriceOperationDto> _inputValidator;
        private readonly IChargePriceConfirmationService _chargePriceConfirmationService;
        private readonly IChargePriceRejectionService _chargePriceRejectionService;
        private readonly IChargePriceNotificationService _chargePriceNotificationService;
        private readonly ILogger _logger;
        private readonly IChargePriceOperationsRejectedEventFactory _chargePriceOperationsRejectedEventFactory;

        public ChargePriceEventHandler(
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IInputValidator<ChargePriceOperationDto> inputValidator,
            IChargePriceConfirmationService chargePriceConfirmationService,
            IChargePriceRejectionService chargePriceRejectionService,
            IChargePriceNotificationService chargePriceNotificationService,
            ILoggerFactory loggerFactory,
            IChargePriceOperationsRejectedEventFactory chargePriceOperationsRejectedEventFactory)
        {
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _inputValidator = inputValidator;
            _chargePriceConfirmationService = chargePriceConfirmationService;
            _chargePriceRejectionService = chargePriceRejectionService;
            _chargePriceNotificationService = chargePriceNotificationService;
            _chargePriceOperationsRejectedEventFactory = chargePriceOperationsRejectedEventFactory;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceEventHandler));
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operations = commandReceivedEvent.Command.Operations.ToArray();
            var document = commandReceivedEvent.Command.Document;
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

                    // Todo: Temporarily stop saving prices in "new flow"
                    // charge.UpdatePrices(
                    //     operation.PointsStartInterval,
                    //     operation.PointsEndInterval,
                    //     operation.Points,
                    //     operation.OperationId);
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

            await _chargePriceConfirmationService.SaveConfirmationsAsync(operationsToBeConfirmed).ConfigureAwait(false);
            RaiseRejectedEvent(commandReceivedEvent, operationsToBeRejected, rejectionRules);
            await _chargePriceNotificationService.SaveNotificationsAsync(operationsToBeConfirmed).ConfigureAwait(false);

            // With story 1411 below log entry will be replaced with 'await _unitOfWork.SaveChangesAsync().ConfigureAwait(false)';
            foreach (var operation in operationsToBeConfirmed)
            {
                _logger.LogInformation("At this point, price(s) will be persisted for operation with Id {Id}", operation.OperationId);
            }
        }

        private void RaiseRejectedEvent(
            ChargePriceCommandReceivedEvent commandReceivedEvent,
            IReadOnlyCollection<ChargePriceOperationDto> operationsToBeRejected,
            IList<IValidationRuleContainer> rejectionRules)
        {
            if (!operationsToBeRejected.Any()) return;
            var chargePriceCommand = new ChargePriceCommand(commandReceivedEvent.Command.Document, operationsToBeRejected);
            var validationResult = ValidationResult.CreateFailure(rejectionRules);

            var rejectedEvent = _chargePriceOperationsRejectedEventFactory.Create(
                chargePriceCommand, validationResult);

            _chargePriceRejectionService.SaveRejections(rejectedEvent);
        }

        private static void CollectRejectionRules(
            List<IValidationRuleContainer> rejectionRules,
            ValidationResult validationResult,
            IEnumerable<ChargePriceOperationDto> operationsToBeRejected,
            ChargePriceOperationDto operation)
        {
            rejectionRules.AddRange(validationResult.InvalidRules);
            rejectionRules.AddRange(operationsToBeRejected.Skip(1)
                .Select(subsequentOperation =>
                    new OperationValidationRuleContainer(
                        new PreviousOperationsMustBeValidRule(operation), subsequentOperation.OperationId)));
        }

        private async Task<Charge?> GetChargeAsync(ChargeOperation chargeOperationDto)
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
