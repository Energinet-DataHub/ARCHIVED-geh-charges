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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Common.Helpers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation
{
    public class ChargeInformationCommandReceivedEventHandler : IChargeCommandReceivedEventHandler
    {
        private readonly ILogger _logger;
        private readonly IClock _clock;
        private readonly IChargeInformationOperationsHandler _chargeInformationOperationsHandler;
        private readonly IDocumentValidator _documentValidator;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IChargeInformationOperationsRejectedEventFactory _chargeInformationOperationsRejectedEventFactory;

        public ChargeInformationCommandReceivedEventHandler(
            IClock clock,
            ILoggerFactory loggerFactory,
            IChargeInformationOperationsHandler chargeInformationOperationsHandler,
            IDocumentValidator documentValidator,
            IDomainEventPublisher domainEventPublisher,
            IChargeInformationOperationsRejectedEventFactory chargeInformationOperationsRejectedEventFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargeInformationCommandReceivedEventHandler));
            _clock = clock;
            _chargeInformationOperationsHandler = chargeInformationOperationsHandler;
            _documentValidator = documentValidator;
            _domainEventPublisher = domainEventPublisher;
            _chargeInformationOperationsRejectedEventFactory = chargeInformationOperationsRejectedEventFactory;
        }

        public async Task HandleAsync(ChargeInformationCommandReceivedEvent chargeInformationCommandReceivedEvent)
        {
            var documentValidationResult = await _documentValidator.ValidateAsync(chargeInformationCommandReceivedEvent.Command).ConfigureAwait(false);
            if (documentValidationResult.IsFailed)
            {
                RaiseRejectedEvent(chargeInformationCommandReceivedEvent, documentValidationResult.InvalidRules);
            }

            await _chargeInformationOperationsHandler.HandleAsync(chargeInformationCommandReceivedEvent).ConfigureAwait(false);
        }

        private void RaiseRejectedEvent(
            ChargeInformationCommandReceivedEvent commandReceivedEvent,
            IList<IValidationRuleContainer> rejectionRules)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                commandReceivedEvent.Command.Document,
                rejectionRules);
            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);
            var validationResult = ValidationResult.CreateFailure(rejectionRules);

            var rejectedEvent = _chargeInformationOperationsRejectedEventFactory.Create(
                commandReceivedEvent.Command.Document,
                commandReceivedEvent.Command.Operations,
                validationResult);

            _domainEventPublisher.Publish(rejectedEvent);
        }
    }
}
