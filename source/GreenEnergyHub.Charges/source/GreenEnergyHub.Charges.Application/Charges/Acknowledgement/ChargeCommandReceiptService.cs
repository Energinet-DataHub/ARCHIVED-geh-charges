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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargeCommandReceiptService : IChargeCommandReceiptService
    {
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;
        private readonly IMessageDispatcher<ChargeCommandRejectedEvent> _rejectedMessageDispatcher;
        private readonly IMessageDispatcher<ChargeCommandAcceptedEvent> _acceptedMessageDispatcher;

        public ChargeCommandReceiptService(
            IChargeCommandRejectedEventFactory chargeCommandRejectedEventFactory,
            IChargeCommandAcceptedEventFactory chargeCommandAcceptedEventFactory,
            IMessageDispatcher<ChargeCommandRejectedEvent> rejectedMessageDispatcher,
            IMessageDispatcher<ChargeCommandAcceptedEvent> acceptedMessageDispatcher)
        {
            _chargeCommandRejectedEventFactory = chargeCommandRejectedEventFactory;
            _chargeCommandAcceptedEventFactory = chargeCommandAcceptedEventFactory;
            _rejectedMessageDispatcher = rejectedMessageDispatcher;
            _acceptedMessageDispatcher = acceptedMessageDispatcher;
        }

        public async Task RejectAsync(ChargeCommand command, ValidationResult validationResult)
        {
            var rejectedEvent = _chargeCommandRejectedEventFactory.CreateEvent(command, validationResult);
            await _rejectedMessageDispatcher.DispatchAsync(rejectedEvent).ConfigureAwait(false);
        }

        public async Task AcceptAsync(ChargeCommand command)
        {
            var acceptedEvent = _chargeCommandAcceptedEventFactory.CreateEvent(command);
            await _acceptedMessageDispatcher.DispatchAsync(acceptedEvent).ConfigureAwait(false);
        }

        /// <summary>
        /// Rejects all invalid operations in a bundle
        /// </summary>
        /// <param name="operationsToBeRejected"></param>
        /// <param name="document"></param>
        /// <param name="rejectionRules"></param>
        public async Task RejectInvalidOperationsAsync(
            IReadOnlyCollection<IChargeOperation> operationsToBeRejected,
            DocumentDto document,
            IList<IValidationRuleContainer> rejectionRules)
        {
            if (operationsToBeRejected.Any())
            {
                await RejectAsync(
                        new ChargeCommand(document, operationsToBeRejected),
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
            IReadOnlyCollection<IChargeOperation> operationsToBeConfirmed,
            DocumentDto document)
        {
            if (operationsToBeConfirmed.Any())
            {
                await AcceptAsync(
                    new ChargeCommand(document, operationsToBeConfirmed)).ConfigureAwait(false);
            }
        }
    }
}
