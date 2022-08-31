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
using GreenEnergyHub.Charges.Application.Common.Helpers;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargeCommandReceiptService : IChargeCommandReceiptService
    {
        private readonly ILogger _logger;
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;
        private readonly IInternalEventDispatcher _internalEventDispatcher;

        public ChargeCommandReceiptService(
            ILoggerFactory loggerFactory,
            IChargeCommandRejectedEventFactory chargeCommandRejectedEventFactory,
            IChargeCommandAcceptedEventFactory chargeCommandAcceptedEventFactory,
            IInternalEventDispatcher internalEventDispatcher)
        {
            _logger = loggerFactory.CreateLogger(nameof(ChargeCommandReceiptService));
            _chargeCommandRejectedEventFactory = chargeCommandRejectedEventFactory;
            _chargeCommandAcceptedEventFactory = chargeCommandAcceptedEventFactory;
            _internalEventDispatcher = internalEventDispatcher;
        }

        public async Task RejectAsync(ChargeInformationCommand command, ValidationResult validationResult)
        {
            var rejectedEvent = _chargeCommandRejectedEventFactory.CreateEvent(command, validationResult);
            await _internalEventDispatcher.DispatchAsync(rejectedEvent).ConfigureAwait(false);
        }

        public async Task AcceptAsync(ChargeInformationCommand command)
        {
            var acceptedEvent = _chargeCommandAcceptedEventFactory.CreateEvent(command);
            await _internalEventDispatcher.DispatchAsync(acceptedEvent).ConfigureAwait(false);
        }

        /// <summary>
        /// Rejects all invalid operations in a bundle
        /// </summary>
        /// <param name="operationsToBeRejected"></param>
        /// <param name="document"></param>
        /// <param name="rejectionRules"></param>
        public async Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRuleContainer> rejectionRules)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                document,
                rejectionRules);
            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);

            if (operationsToBeRejected.Any())
            {
                await RejectAsync(
                        new ChargeInformationCommand(document, operationsToBeRejected),
                        ValidationResult.CreateFailure(rejectionRules))
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Accepts all valid operations in a bundle
        /// </summary>
        /// <param name="operationsToBeConfirmed"></param>
        /// <param name="document"></param>
        public async Task AcceptValidOperationsAsync(
            IReadOnlyCollection<ChargeInformationOperationDto> operationsToBeConfirmed,
            DocumentDto document)
        {
            if (operationsToBeConfirmed.Any())
            {
                await AcceptAsync(
                    new ChargeInformationCommand(document, operationsToBeConfirmed)).ConfigureAwait(false);
            }
        }
    }
}
