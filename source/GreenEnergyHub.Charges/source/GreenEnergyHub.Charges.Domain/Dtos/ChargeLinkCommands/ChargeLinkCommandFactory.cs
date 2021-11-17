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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.CreateLinkRequest;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands
{
    public class ChargeLinkCommandFactory : IChargeLinkCommandFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IClock _clock;
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeLinkCommandFactory(
            IChargeRepository chargeRepository,
            IMeteringPointRepository meteringPointRepository,
            IClock clock,
            IMarketParticipantRepository marketParticipantRepository)
        {
            _chargeRepository = chargeRepository;
            _clock = clock;
            _marketParticipantRepository = marketParticipantRepository;
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task<ChargeLinksCommand> CreateAsync(
            [NotNull] CreateLinkCommandEvent createLinkCommandEvent,
            [NotNull] IReadOnlyCollection<DefaultChargeLink> defaultChargeLinks)
        {
            var charges = await _chargeRepository.GetChargesAsync(
                defaultChargeLinks.Select(x => x.ChargeId).ToList()).ConfigureAwait(false);
            var meteringPoint = await _meteringPointRepository.GetMeteringPointAsync(
                    createLinkCommandEvent.MeteringPointId)
                .ConfigureAwait(false);
            var systemOperator = await _marketParticipantRepository.GetSystemOperatorAsync().ConfigureAwait(false);

            var defChargeAndCharge =
                defaultChargeLinks.ToDictionary(defaultChargeLink => defaultChargeLink, defaultChargeLink =>
                    charges.Single(c => defaultChargeLink.ChargeId == c.Id));

            var chargeLinks = defChargeAndCharge.Select(pair => new ChargeLinkDto
                {
                    ChargeType = pair.Value.Type,
                    SenderProvidedChargeId = pair.Value.SenderProvidedChargeId,
                    EndDateTime = pair.Key.EndDateTime,
                    ChargeOwner = pair.Value.Owner,
                    StartDateTime = pair.Key.GetStartDateTime(meteringPoint.EffectiveDate),
                    OperationId = Guid.NewGuid().ToString(),
                    Factor = DefaultChargeLink.Factor,
                })
                .ToList();

            var currentTime = _clock.GetCurrentInstant();
            return new ChargeLinksCommand(
                createLinkCommandEvent.MeteringPointId,
                new DocumentDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = DocumentType.RequestChangeBillingMasterData,
                    IndustryClassification = IndustryClassification.Electricity,
                    BusinessReasonCode = BusinessReasonCode.UpdateMasterDataSettlement,
                    RequestDate = currentTime,
                    CreatedDateTime = currentTime,
                    Sender = new MarketParticipant
                    {
                        Id = systemOperator.Id, // For default charge links the owner is the TSO.
                        BusinessProcessRole = MarketParticipantRole.SystemOperator,
                    },
                    Recipient = new MarketParticipant
                    {
                        Id = DefaultChargeLink.GlnNumber,
                        BusinessProcessRole = MarketParticipantRole.MeteringPointAdministrator,
                    },
                },
                chargeLinks);
        }
    }
}
