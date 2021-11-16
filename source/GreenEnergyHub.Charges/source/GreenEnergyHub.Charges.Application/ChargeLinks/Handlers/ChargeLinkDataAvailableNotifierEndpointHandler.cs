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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.DefaultChargeLinksDataAvailableNotifiedEvents;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinkDataAvailableNotifierEndpointHandler : IChargeLinkDataAvailableNotifierEndpointHandler
    {
        private readonly IMessageDispatcher<DefaultChargeLinksDataAvailableNotifierEvent> _messageDispatcher;
        private readonly IDefaultChargeLinkDataAvailableNotifierEventFactory _defaultChargeLinkDataAvailableNotifierEventFactory;

        public ChargeLinkDataAvailableNotifierEndpointHandler(
            IMessageDispatcher<DefaultChargeLinksDataAvailableNotifierEvent> messageDispatcher,
            IDefaultChargeLinkDataAvailableNotifierEventFactory defaultChargeLinkDataAvailableNotifierEventFactory)
        {
            _messageDispatcher = messageDispatcher;
            _defaultChargeLinkDataAvailableNotifierEventFactory = defaultChargeLinkDataAvailableNotifierEventFactory;
        }

        public async Task HandleAsync(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            var chargeLinkDataAvailableNotifierEvent =
                _defaultChargeLinkDataAvailableNotifierEventFactory.CreteFromAcceptedEvent(chargeLinkCommandAcceptedEvent);
            await _messageDispatcher.DispatchAsync(chargeLinkDataAvailableNotifierEvent);
        }
    }
}
