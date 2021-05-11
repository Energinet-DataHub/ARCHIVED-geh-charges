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
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandAcknowledgementService : IChargeCommandAcknowledgementService
    {
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;
        private readonly IMessageDispatcher<ChargeCommandRejectedEvent> _rejectedMessageDispatcher;
        private readonly IMessageDispatcher<ChargeCommandAcceptedEvent> _acceptedMessageDispatcher;

        public ChargeCommandAcknowledgementService(
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

        public async Task RejectAsync(ChargeCommand command, ChargeCommandValidationResult validationResult)
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
