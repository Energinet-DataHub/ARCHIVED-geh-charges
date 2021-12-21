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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargeCommandConfirmationService : IChargeCommandConfirmationService
    {
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;
        private readonly IMessageDispatcher<ChargeCommandRejectedEvent> _rejectedMessageDispatcher;
        private readonly IMessageDispatcher<ChargeCommandAcceptedEvent> _acceptedMessageDispatcher;

        public ChargeCommandConfirmationService(
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
    }
}
