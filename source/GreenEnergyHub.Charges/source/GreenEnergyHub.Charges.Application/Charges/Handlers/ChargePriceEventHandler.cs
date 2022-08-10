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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Charges.Exceptions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
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

        public ChargePriceEventHandler(
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IInputValidator<ChargePriceOperationDto> inputValidator,
            IChargePriceConfirmationService chargePriceConfirmationService,
            IChargePriceRejectionService chargePriceRejectionService,
            IChargePriceNotificationService chargePriceNotificationService,
            ILoggerFactory loggerFactory)
        {
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _inputValidator = inputValidator;
            _chargePriceConfirmationService = chargePriceConfirmationService;
            _chargePriceRejectionService = chargePriceRejectionService;
            _chargePriceNotificationService = chargePriceNotificationService;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceEventHandler));
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent commandReceivedEvent)
        {
            ArgumentNullException.ThrowIfNull(commandReceivedEvent);

            var operations = commandReceivedEvent.Command.Operations.ToArray();
            var operationsToBeRejected = new List<ChargePriceOperationDto>();
            var rejectionRules = new List<IValidationRuleContainer>();
            var operationsToBeConfirmed = new List<ChargePriceOperationDto>();

            for (var i = 0; i < operations.Length; i++)
            {
                var operation = operations[i];

                var inputValidationResult = _inputValidator.Validate(operation);
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
                        throw new InvalidOperationException($"Charge ID '{operation.ChargeId}' does not exist.");
                    }

                    // Temporary stop saving prices in "new flow"
                    // charge.UpdatePrices(
                    //     operation.PointsStartInterval,
                    //     operation.PointsEndInterval,
                    //     operation.Points,
                    //     operation.Id);
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
                _logger.LogInformation("At this point, price(s) will be persisted for operation with Id {Id}", operation.Id);
            }
        }

        private void RaiseRejectedEvent(
            ChargePriceCommandReceivedEvent commandReceivedEvent,
            List<ChargePriceOperationDto> operationsToBeRejected,
            List<IValidationRuleContainer> rejectionRules)
        {
            if (!operationsToBeRejected.Any()) return;

            var sender = commandReceivedEvent.Command.Document.Sender;
            var rejectEvent = new OperationsRejectedEvent(
                operationsToBeRejected,
                sender.MarketParticipantId,
                sender.BusinessProcessRole,
                rejectionRules);
            _chargePriceRejectionService.SaveRejections(rejectEvent);
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
                        new PreviousOperationsMustBeValidRule(operation.Id), subsequentOperation.Id)));
        }

        private async Task<Charge?> GetChargeAsync(ChargePriceOperationDto chargeOperationDto)
        {
            var marketParticipant = await _marketParticipantRepository
                .GetSystemOperatorOrGridAccessProviderAsync(chargeOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                marketParticipant.Id,
                chargeOperationDto.Type);
            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
