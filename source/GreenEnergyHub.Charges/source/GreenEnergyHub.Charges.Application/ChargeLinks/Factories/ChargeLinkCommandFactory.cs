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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Command;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Integration;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Factories
{
    public class ChargeLinkCommandFactory : IChargeLinkCommandFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IClock _clock;

        public ChargeLinkCommandFactory(IChargeRepository chargeRepository, IClock clock)
        {
            _chargeRepository = chargeRepository;
            _clock = clock;
        }

        public async Task<ChargeLinkCommand> CreateAsync(
            [NotNull] CreateLinkCommandEvent createLinkCommandEvent,
            [NotNull] DefaultChargeLink defaultChargeLink,
            string correlationId)
        {
            var charge = await _chargeRepository.GetChargeAsync(defaultChargeLink.ChargeRowId).ConfigureAwait(false);
            var currentTime = _clock.GetCurrentInstant();
            var chargeLinkCommand = new ChargeLinkCommand(correlationId)
            {
                Document = new Document
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = DocumentType.RequestChangeBillingMasterData,
                    IndustryClassification = IndustryClassification.Electricity,
                    BusinessReasonCode = BusinessReasonCode.UpdateMasterDataSettlement,
                    RequestDate = currentTime,
                    CreatedDateTime = currentTime,
                    Sender = new MarketParticipant
                    {
                        Id = charge.Owner, // For default charge links the owner is the TSO.
                        BusinessProcessRole = MarketParticipantRole.SystemOperator,
                    },
                    Recipient = new MarketParticipant
                    {
                        Id = DefaultChargeLink.GlnNumber,
                        BusinessProcessRole = MarketParticipantRole.MeteringPointAdministrator,
                    },
                },
                ChargeLink = new ChargeLinkDto
                {
                    ChargeType = charge.Type,
                    ChargeId = charge.Id,
                    EndDateTime = defaultChargeLink.EndDateTime,
                    ChargeOwner = charge.Owner,
                    MeteringPointId = createLinkCommandEvent.MeteringPointId,
                    StartDateTime = defaultChargeLink.GetStartDateTime(createLinkCommandEvent.StartDateTime),
                    OperationId = Guid.NewGuid().ToString(),
                    Factor = DefaultChargeLink.Factor,
                },
            };
            return chargeLinkCommand;
        }
    }
}
