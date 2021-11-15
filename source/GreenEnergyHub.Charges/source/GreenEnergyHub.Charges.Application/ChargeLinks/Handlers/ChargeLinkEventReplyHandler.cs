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

using System;
using System.Linq;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class ChargeLinkEventReplyHandler : IChargeLinkEventReplyHandler
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IDefaultChargeLinkClient _defaultChargeLinkClient;
        private readonly ICorrelationContext _correlationContext;

        public ChargeLinkEventReplyHandler(
            IMessageMetaDataContext messageMetaDataContext,
            IDefaultChargeLinkClient defaultChargeLinkClient,
            ICorrelationContext correlationContext)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _defaultChargeLinkClient = defaultChargeLinkClient;
            _correlationContext = correlationContext;
        }

        public async Task HandleAsync(ChargeLinkCommandAcceptedEvent command)
        {
                CheckAllMeteringPointIdsAreTheSame(command);

                // TODO:  A refactor of ChargeLinkCommands will end with the commands being wrapped by a entity with only one meteringPointId.
                var meteringPointId = command.ChargeLinkCommands.First().ChargeLink.MeteringPointId;

                await _defaultChargeLinkClient
                    .CreateDefaultChargeLinksSucceededReplyAsync(
                        new CreateDefaultChargeLinksSucceededDto(
                            meteringPointId,
                            true),
                        _correlationContext.Id,
                        _messageMetaDataContext.ReplyTo).ConfigureAwait(false);
        }

        private static void CheckAllMeteringPointIdsAreTheSame(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            var allChargeLinkMeteringPointIdsAreTheSame = chargeLinkCommandAcceptedEvent.ChargeLinkCommands
                .All(c => c.ChargeLink.MeteringPointId == chargeLinkCommandAcceptedEvent.ChargeLinkCommands
                    .First().ChargeLink.MeteringPointId);

            if (!allChargeLinkMeteringPointIdsAreTheSame)
            {
                throw new InvalidOperationException($"not all metering point Ids are the same on {nameof(ChargeLinkCommandAcceptedEvent)}");
            }
        }
    }
}
