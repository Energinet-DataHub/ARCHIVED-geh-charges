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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Services
{
    public class ChargeLinksReceiptService : IChargeLinksReceiptService
    {
        private readonly IInternalEventDispatcher _internalEventDispatcher;
        private readonly IChargeLinksAcceptedEventFactory _chargeLinksAcceptedEventFactory;
        private readonly IChargeLinksRejectedEventFactory _chargeLinksRejectedEventFactory;

        public ChargeLinksReceiptService(
            IInternalEventDispatcher internalEventDispatcher,
            IChargeLinksAcceptedEventFactory chargeLinksAcceptedEventFactory,
            IChargeLinksRejectedEventFactory chargeLinksRejectedEventFactory)
        {
            _internalEventDispatcher = internalEventDispatcher;
            _chargeLinksAcceptedEventFactory = chargeLinksAcceptedEventFactory;
            _chargeLinksRejectedEventFactory = chargeLinksRejectedEventFactory;
        }

        public async Task RejectAsync(ChargeLinksCommand command, ValidationResult validationResult)
        {
            var rejectedEvent = _chargeLinksRejectedEventFactory.Create(command, validationResult);
            await _internalEventDispatcher.DispatchAsync(rejectedEvent).ConfigureAwait(false);
        }

        public async Task AcceptAsync(ChargeLinksCommand command)
        {
            var acceptedEvent = _chargeLinksAcceptedEventFactory.Create(command);
            await _internalEventDispatcher.DispatchAsync(acceptedEvent).ConfigureAwait(false);
        }
    }
}
