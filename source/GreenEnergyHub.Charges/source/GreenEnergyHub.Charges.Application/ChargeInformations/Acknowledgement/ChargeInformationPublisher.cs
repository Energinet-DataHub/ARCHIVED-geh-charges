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
using GreenEnergyHub.Charges.Application.ChargeInformations.Factories;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;

namespace GreenEnergyHub.Charges.Application.ChargeInformations.Acknowledgement
{
    public class ChargeInformationPublisher : IChargeInformationPublisher
    {
        private readonly IMessageDispatcher<ChargeInformationCreatedEvent> _messageChargeDispatcher;
        private readonly IChargeInformationCreatedEventFactory _chargeInformationCreatedEventFactory;

        public ChargeInformationPublisher(
            IMessageDispatcher<ChargeInformationCreatedEvent> messageChargeDispatcher,
            IChargeInformationCreatedEventFactory chargeInformationCreatedEventFactory)
        {
            _messageChargeDispatcher = messageChargeDispatcher;
            _chargeInformationCreatedEventFactory = chargeInformationCreatedEventFactory;
        }

        public async Task PublishChargeInformationCreatedAsync(ChargeOperationDto chargeOperationDto)
        {
            var chargeCreatedEvent = _chargeInformationCreatedEventFactory.Create(chargeOperationDto);
            await _messageChargeDispatcher.DispatchAsync(chargeCreatedEvent).ConfigureAwait(false);
        }

        public Task PublishChargeUpdatedAsync(ChargeCommandAcceptedEvent chargeCommandAcceptedEvent)
        {
            return Task.CompletedTask;
        }
    }
}
