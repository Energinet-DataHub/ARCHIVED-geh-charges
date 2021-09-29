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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.ChargeLinks;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinkCommandReceivedHandler : IChargeLinkCommandReceivedHandler
    {
        private readonly IMessageDispatcher<ChargeLinkCommandAcceptedEvent> _messageDispatcher;
        private readonly IChargeLinkCommandAcceptedEventFactory _chargeLinkCommandAcceptedEventFactory;
        private readonly IChargeLinkFactory _chargeLinkFactory;
        private readonly IChargeLinkRepository _chargeLinkRepository;
        private readonly IChargeLinkCommandFactory _chargeLinkCommandFactory;

        public ChargeLinkCommandReceivedHandler(
            IMessageDispatcher<ChargeLinkCommandAcceptedEvent> messageDispatcher,
            IChargeLinkCommandAcceptedEventFactory chargeLinkCommandAcceptedEventFactory,
            IChargeLinkFactory chargeLinkFactory,
            IChargeLinkRepository chargeLinkRepository,
            IChargeLinkCommandFactory chargeLinkCommandFactory)
        {
            _messageDispatcher = messageDispatcher;
            _chargeLinkCommandAcceptedEventFactory = chargeLinkCommandAcceptedEventFactory;
            _chargeLinkFactory = chargeLinkFactory;
            _chargeLinkRepository = chargeLinkRepository;
            _chargeLinkCommandFactory = chargeLinkCommandFactory;
        }

        public async Task HandleAsync([NotNull] ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            // Upcoming stories will cover the update scenarios where charge link already exists
            var chargeLink = await _chargeLinkFactory.CreateAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);
            await _chargeLinkRepository.StoreAsync(chargeLink).ConfigureAwait(false);

            var chargeLinkCommandAcceptedEvents = new List<ChargeLinkCommandAcceptedEvent>();

            foreach (var period in chargeLink.PeriodDetails)
            {
                var chargeLinkCommand = await _chargeLinkCommandFactory.CreateFromChargeLinkAsync(
                        chargeLink,
                        period,
                        chargeLinkCommandReceivedEvent.CorrelationId)
                    .ConfigureAwait(false);

                chargeLinkCommandAcceptedEvents.Add(_chargeLinkCommandAcceptedEventFactory.Create(
                    chargeLinkCommand, chargeLinkCommandReceivedEvent.CorrelationId));
            }

            foreach (var chargeLinkCommandAcceptedEvent in chargeLinkCommandAcceptedEvents)
            {
                await _messageDispatcher.DispatchAsync(chargeLinkCommandAcceptedEvent).ConfigureAwait(false);
            }
        }
    }
}
