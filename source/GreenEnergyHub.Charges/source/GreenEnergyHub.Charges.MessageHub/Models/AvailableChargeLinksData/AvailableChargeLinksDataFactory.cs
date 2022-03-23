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
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData
{
    public class AvailableChargeLinksDataFactory
        : AvailableDataFactoryBase<AvailableChargeLinksData, ChargeLinksAcceptedEvent>
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public AvailableChargeLinksDataFactory(
            IMarketParticipantRepository marketParticipantRepository,
            IChargeRepository chargeRepository,
            IMessageMetaDataContext messageMetaDataContext)
            : base(marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _chargeRepository = chargeRepository;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public override async Task<IReadOnlyList<AvailableChargeLinksData>> CreateAsync(
            ChargeLinksAcceptedEvent acceptedEvent)
        {
            // It is the responsibility of the Charge Domain to find the recipient and
            // not considered part of the Create Metering Point orchestration.
            // We select the first as all bundled messages will have the same recipient
            var recipient = await _marketParticipantRepository
                .GetGridAccessProviderAsync(acceptedEvent.ChargeLinksCommand.MeteringPointId).ConfigureAwait(false);

            var result = new List<AvailableChargeLinksData>();

            foreach (var link in acceptedEvent.ChargeLinksCommand.ChargeLinks)
            {
                var chargeIdentifier = new ChargeIdentifier(link.SenderProvidedChargeId, link.ChargeOwner, link.ChargeType);
                var charge = await _chargeRepository.GetAsync(chargeIdentifier).ConfigureAwait(false);
                var sender = await GetSenderAsync().ConfigureAwait(false);
                if (ShouldMakeDataAvailableForGridOwnerOfMeteringPoint(charge))
                {
                    result.Add(new AvailableChargeLinksData(
                        sender.MarketParticipantId,
                        sender.BusinessProcessRole,
                        recipient.MarketParticipantId,
                        recipient.BusinessProcessRole,
                        acceptedEvent.ChargeLinksCommand.Document.BusinessReasonCode,
                        _messageMetaDataContext.RequestDataTime,
                        Guid.NewGuid(), // ID of each available piece of data must be unique
                        link.SenderProvidedChargeId,
                        link.ChargeOwner,
                        link.ChargeType,
                        acceptedEvent.ChargeLinksCommand.MeteringPointId,
                        link.Factor,
                        link.StartDateTime,
                        link.EndDateTime.GetValueOrDefault(),
                        acceptedEvent.ChargeLinksCommand.Document.Type,
                        0));
                }
            }

            return result;
        }

        private bool ShouldMakeDataAvailableForGridOwnerOfMeteringPoint(Charge charge)
        {
            // We only need to notify the grid provider owning the metering point if
            // the link is about a tax charge; the rest they maintain themselves
            return charge.TaxIndicator;
        }
    }
}
