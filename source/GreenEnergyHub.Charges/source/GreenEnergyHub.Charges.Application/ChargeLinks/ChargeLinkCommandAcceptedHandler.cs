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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Mapping;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Application.ChargeLinks
{
    public class ChargeLinkCommandAcceptedHandler : IChargeLinkCommandAcceptedHandler
    {
        private readonly MessageDispatcher _messageDispatcher;
        private readonly IChargeLinkCommandMapper _chargeLinkCommandMapper;

        public ChargeLinkCommandAcceptedHandler(
            MessageDispatcher messageDispatcher,
            IChargeLinkCommandMapper chargeLinkCommandMapper)
        {
            _messageDispatcher = messageDispatcher;
            _chargeLinkCommandMapper = chargeLinkCommandMapper;
        }

        public async Task HandleAsync([NotNull] ChargeLinkCommandReceivedEvent chargeLinkCommand)
        {
            var chargeCommandAcceptedEvent = _chargeLinkCommandMapper.Map(chargeLinkCommand);

            // Dispatch
            await _messageDispatcher.DispatchAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);
        }
    }
}
