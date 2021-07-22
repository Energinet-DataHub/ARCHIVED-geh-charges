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
using GreenEnergyHub.Charges.Application.Factories;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks
{
    public class ChargeLinkEventPublishHandler : IChargeLinkEventPublishHandler
    {
        private readonly IMessageDispatcher<ChargeLinkCreatedEvent> _createdDispatcher;
        private readonly IChargeLinkCreatedEventFactory _createdEventFactory;

        public ChargeLinkEventPublishHandler(
            IChargeLinkCreatedEventFactory createdEventFactory,
            IMessageDispatcher<ChargeLinkCreatedEvent> createdDispatcher)
        {
            _createdEventFactory = createdEventFactory;
            _createdDispatcher = createdDispatcher;
        }

        public async Task HandleAsync(ChargeLinkCommand command)
        {
            var createdEvent = _createdEventFactory.CreateEvent(command);

            await _createdDispatcher.DispatchAsync(createdEvent).ConfigureAwait(false);
        }
    }
}
