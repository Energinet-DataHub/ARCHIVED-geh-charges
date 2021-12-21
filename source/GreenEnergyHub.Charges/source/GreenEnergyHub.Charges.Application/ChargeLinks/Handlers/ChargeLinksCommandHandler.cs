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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinksCommandHandler : IChargeLinksCommandHandler
    {
        private readonly IMessageDispatcher<ChargeLinksReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;

        public ChargeLinksCommandHandler(
            IMessageDispatcher<ChargeLinksReceivedEvent> messageDispatcher,
            IClock clock)
        {
            _messageDispatcher = messageDispatcher;
            _clock = clock;
        }

        public async Task HandleAsync(ChargeLinksCommand command)
        {
            var receivedEvent = new ChargeLinksReceivedEvent(
                _clock.GetCurrentInstant(),
                command);

            await _messageDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);
        }
    }
}
