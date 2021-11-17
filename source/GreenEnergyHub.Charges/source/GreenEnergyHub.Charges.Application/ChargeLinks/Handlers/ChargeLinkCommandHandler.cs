﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers.Message;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinkCommandHandler : IChargeLinkCommandHandler
    {
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;

        public ChargeLinkCommandHandler(
            IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher,
            IClock clock)
        {
            _messageDispatcher = messageDispatcher;
            _clock = clock;
        }

        public async Task<ChargeLinksMessageResult> HandleAsync(ChargeLinkCommand command)
        {
            var receivedEvent = new ChargeLinkCommandReceivedEvent(
                _clock.GetCurrentInstant(),
                new List<ChargeLinkCommand> { command });

            await _messageDispatcher.DispatchAsync(receivedEvent).ConfigureAwait(false);

            var chargeLinksMessageResult = ChargeLinksMessageResult.CreateSuccess();

            return chargeLinksMessageResult;
        }
    }
}
