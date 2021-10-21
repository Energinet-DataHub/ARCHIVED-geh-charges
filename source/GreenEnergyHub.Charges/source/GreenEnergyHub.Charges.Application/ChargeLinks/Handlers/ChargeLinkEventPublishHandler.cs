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

using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCreatedEvents;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinkEventPublishHandler : IChargeLinkEventPublishHandler
    {
        private readonly IMessageDispatcher<ChargeLinkCreatedEvent> _createdDispatcher;
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IDefaultChargeLinkClient _defaultChargeLinkClient;
        private readonly ICorrelationContext _correlationContext;
        private readonly IChargeLinkCreatedEventFactory _createdEventFactory;

        public ChargeLinkEventPublishHandler(
            IChargeLinkCreatedEventFactory createdEventFactory,
            IMessageDispatcher<ChargeLinkCreatedEvent> createdDispatcher,
            IMessageMetaDataContext messageMetaDataContext,
            IDefaultChargeLinkClient defaultChargeLinkClient,
            ICorrelationContext correlationContext)
        {
            _createdEventFactory = createdEventFactory;
            _createdDispatcher = createdDispatcher;
            _messageMetaDataContext = messageMetaDataContext;
            _defaultChargeLinkClient = defaultChargeLinkClient;
            _correlationContext = correlationContext;
        }

        public async Task HandleAsync(ChargeLinkCommandAcceptedEvent command)
        {
            if (_messageMetaDataContext.ReplyTo != null)
            {
                // A refactor of ChargeLinkCommands will end with the commands being wrapped by a entity with only one meteringPointId.
                var meteringPointId = command.ChargeLinkCommands.First().ChargeLink.MeteringPointId;

                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksSucceededReplyAsync(
                        new CreateDefaultChargeLinksSucceededDto(
                            meteringPointId,
                            true),
                        _correlationContext.Id,
                        _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
            }

            var chargeLinkCreatedEvents =
                command.ChargeLinkCommands.Select(
                    chargeLinkCommand => _createdEventFactory.CreateEvent(chargeLinkCommand)).ToList();

            await Task.WhenAll(
                chargeLinkCreatedEvents
                    .Select(chargeLinkCreatedEvent
                        => _createdDispatcher.DispatchAsync(chargeLinkCreatedEvent))).ConfigureAwait(false);
        }
    }
}
