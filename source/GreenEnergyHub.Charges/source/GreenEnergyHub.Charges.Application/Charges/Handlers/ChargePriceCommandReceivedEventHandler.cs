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
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Common.Helpers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargePriceCommandReceivedEventHandler : IChargePriceCommandReceivedEventHandler
    {
        private readonly ILogger _logger;
        private readonly IChargePriceEventHandler _chargePriceEventHandler;
        private readonly IDocumentValidator _documentValidator;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IChargeEventFactory _chargeEventFactory;

        public ChargePriceCommandReceivedEventHandler(
            ILoggerFactory loggerFactory,
            IChargePriceEventHandler chargePriceEventHandler,
            IDocumentValidator documentValidator,
            IDomainEventPublisher domainEventPublisher,
            IChargeEventFactory chargeEventFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargePriceCommandReceivedEventHandler));
            _chargePriceEventHandler = chargePriceEventHandler;
            _documentValidator = documentValidator;
            _domainEventPublisher = domainEventPublisher;
            _chargeEventFactory = chargeEventFactory;
        }

        public async Task HandleAsync(ChargePriceCommandReceivedEvent chargePriceCommandReceivedEvent)
        {
            var documentValidationResult = await _documentValidator
                .ValidateAsync(chargePriceCommandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                RaiseRejectedEvent(chargePriceCommandReceivedEvent, documentValidationResult.InvalidRules.ToList());
                return;
            }

            await _chargePriceEventHandler.HandleAsync(chargePriceCommandReceivedEvent).ConfigureAwait(false);
        }

        private void RaiseRejectedEvent(
            ChargePriceCommandReceivedEvent commandReceivedEvent,
            IList<IValidationRuleContainer> rejectionRules)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                commandReceivedEvent.Command.Document,
                rejectionRules);
            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);
            var validationResult = ValidationResult.CreateFailure(rejectionRules);

            var rejectedEvent = _chargeEventFactory.CreatePriceRejectedEvent(
                commandReceivedEvent.Command.Document,
                commandReceivedEvent.Command.Operations,
                validationResult);

            _domainEventPublisher.Publish(rejectedEvent);
        }
    }
}
