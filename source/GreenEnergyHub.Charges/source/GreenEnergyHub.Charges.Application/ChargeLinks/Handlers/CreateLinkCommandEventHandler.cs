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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.CreateLinkCommandEvents;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkCommandEventHandler : ICreateLinkCommandEventHandler
    {
        private readonly IDefaultChargeLinkRepository _defaultChargeLinkRepository;
        private readonly IChargeLinkCommandFactory _chargeLinkCommandFactory;
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;

        public CreateLinkCommandEventHandler(
            IDefaultChargeLinkRepository defaultChargeLinkRepository,
            IChargeLinkCommandFactory chargeLinkCommandFactory,
            IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher,
            IClock clock)
        {
            _defaultChargeLinkRepository = defaultChargeLinkRepository;
            _chargeLinkCommandFactory = chargeLinkCommandFactory;
            _messageDispatcher = messageDispatcher;
            _clock = clock;
        }

        public async Task HandleAsync([NotNull] CreateLinkCommandEvent createLinkCommandEvent, string correlationId)
        {
            var defaultChargeLinks = await _defaultChargeLinkRepository
                .GetAsync(createLinkCommandEvent.MeteringPointType).ConfigureAwait(false);

            foreach (var defaultChargeLink in defaultChargeLinks)
            {
                if (defaultChargeLink.ApplicableForLinking(
                    createLinkCommandEvent.StartDateTime,
                    createLinkCommandEvent.MeteringPointType))
                {
                    var chargeLinkCommand =
                        await _chargeLinkCommandFactory.CreateAsync(
                            createLinkCommandEvent,
                            defaultChargeLink,
                            correlationId).ConfigureAwait(false);

                    var chargeLinkCommandReceivedEvent = new ChargeLinkCommandReceivedEvent(
                        _clock.GetCurrentInstant(),
                        correlationId,
                        chargeLinkCommand);

                    await _messageDispatcher.DispatchAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);
                }
            }
        }
    }
}
