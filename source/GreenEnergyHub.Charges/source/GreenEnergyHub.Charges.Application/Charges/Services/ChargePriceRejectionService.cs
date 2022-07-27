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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Services
{
    public class ChargePriceRejectionService : IChargePriceRejectionService
    {
        private readonly ChargePriceRejectedEventFactory _chargePriceRejectedEventFactory;
        private readonly IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceRejectedEvent> _availableDataNotifier;
        private readonly ILogger _logger;

        public ChargePriceRejectionService(
            ILoggerFactory loggerFactory,
            ChargePriceRejectedEventFactory chargePriceRejectedEventFactory,
            IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceRejectedEvent> availableDataNotifier)
        {
            _chargePriceRejectedEventFactory = chargePriceRejectedEventFactory;
            _availableDataNotifier = availableDataNotifier;
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceConfirmationService));
        }

        public async Task SaveRejectionsAsync(
            DocumentDto document,
            List<ChargePriceOperationDto> rejectedPriceOperations,
            ValidationResult validationResult)
        {
            var command = new ChargePriceCommand(document, rejectedPriceOperations);
            var rejectedEvent = _chargePriceRejectedEventFactory.CreateEvent(command, validationResult);
            await _availableDataNotifier.NotifyAsync(rejectedEvent).ConfigureAwait(false);
            foreach (var chargePriceOperationDto in rejectedPriceOperations)
            {
                _logger.LogInformation(
                    $"{chargePriceOperationDto.ChargeId} rejected price operations was persisted. With errors: {PrintInvalidRules(validationResult)}");
            }
        }

        public async Task SaveRejectionsAsync(
            DocumentDto document,
            List<ChargePriceOperationDto> operationsToBeRejected,
            List<IValidationRuleContainer> validationResult)
        {
            var command = new ChargePriceCommand(document, operationsToBeRejected);
            var rejectedEvent = _chargePriceRejectedEventFactory.CreateEvent(command, ValidationResult.CreateFailure(validationResult));
            await _availableDataNotifier.NotifyAsync(rejectedEvent).ConfigureAwait(false);
            foreach (var chargePriceOperationDto in operationsToBeRejected)
            {
                _logger.LogInformation(
                    $"{chargePriceOperationDto.ChargeId} rejected price operations was persisted. With errors: {PrintInvalidRules(ValidationResult.CreateFailure(validationResult))}");
            }
        }

        private static string PrintInvalidRules(ValidationResult documentValidationResult)
        {
            return string.Join(
                ",",
                documentValidationResult.InvalidRules.Select(x =>
                    x.ValidationRule.ValidationRuleIdentifier.ToString()));
        }
    }
}
