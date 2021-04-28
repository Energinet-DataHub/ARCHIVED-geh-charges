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
using GreenEnergyHub.Charges.Application.ChangeOfCharges;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeCommandAcknowledgementService : IChargeCommandAcknowledgementService
    {
        private readonly IInternalEventPublisher _internalEventPublisher;
        private readonly IChargeCommandRejectedEventFactory _chargeCommandRejectedEventFactory;
        private readonly IChargeCommandAcceptedEventFactory _chargeCommandAcceptedEventFactory;

        public ChargeCommandAcknowledgementService(
            IInternalEventPublisher internalEventPublisher,
            IChargeCommandRejectedEventFactory chargeCommandRejectedEventFactory,
            IChargeCommandAcceptedEventFactory chargeCommandAcceptedEventFactory)
        {
            _internalEventPublisher = internalEventPublisher;
            _chargeCommandRejectedEventFactory = chargeCommandRejectedEventFactory;
            _chargeCommandAcceptedEventFactory = chargeCommandAcceptedEventFactory;
        }

        public async Task RejectAsync(ChargeCommand command, ChargeCommandValidationResult validationResult)
        {
            var chargeEvent = _chargeCommandRejectedEventFactory.CreateEvent(command);
            await _internalEventPublisher.PublishAsync(chargeEvent).ConfigureAwait(false);
        }

        public async Task AcceptAsync(ChargeCommand command)
        {
            var chargeEvent = _chargeCommandAcceptedEventFactory.CreateEvent(command);
            await _internalEventPublisher.PublishAsync(chargeEvent).ConfigureAwait(false);
        }
    }
}
