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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.Charges.Handlers
{
    public class ChargeInformationCommandHandler : IChargeInformationCommandHandler
    {
        private readonly IClock _clock;
        private readonly IInternalEventDispatcher<ChargeCommandReceivedEvent> _internalEventDispatcher;

        public ChargeInformationCommandHandler(
            IClock clock,
            IInternalEventDispatcher<ChargeCommandReceivedEvent> internalEventDispatcher)
        {
            _clock = clock;
            _internalEventDispatcher = internalEventDispatcher;
        }

        public async Task HandleAsync(ChargeInformationCommand command)
        {
            var receivedEvent = new ChargeCommandReceivedEvent(_clock.GetCurrentInstant(), command);
            await _internalEventDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);
        }
    }
}
