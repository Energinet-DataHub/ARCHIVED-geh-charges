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
using GreenEnergyHub.Charges.Domain.Dtos.CreateDefaultChargeLinksRequests;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands
{
    public class ChargeLinksCommandFactory : IChargeLinksCommandFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IClock _clock;
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ChargeLinksCommandFactory(
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
            [NotNull] CreateDefaultChargeLinksRequest createDefaultChargeLinksRequest,
            [NotNull] IReadOnlyCollection<DefaultChargeLink> defaultChargeLinks)
        {
            var chargeIds = defaultChargeLinks.Select(x => x.ChargeId).ToList();

            var charges = await _chargeRepository
                .GetAsync(chargeIds)
                .ConfigureAwait(false);

            var ownerIds = charges.Select(c => c.OwnerId);
            var owners = await _marketParticipantRepository
                .GetAsync(ownerIds)
                .ConfigureAwait(false);

            var meteringPoint = await _meteringPointRepository
                .GetMeteringPointAsync(createDefaultChargeLinksRequest.MeteringPointId)
                .ConfigureAwait(false);

            var systemOperator = await _marketParticipantRepository
                .GetSystemOperatorAsync()
                .ConfigureAwait(false);

            var defChargeAndCharge =
                defaultChargeLinks
                    .ToDictionary(
                        defaultChargeLink => defaultChargeLink,
                        defaultChargeLink => charges.Single(c => defaultChargeLink.ChargeId == c.Id));

            var chargeLinks = defChargeAndCharge.Select(pair => new ChargeLinkDto
                {
                    ChargeType = pair.Value.Type,
                    SenderProvidedChargeId = pair.Value.SenderProvidedChargeId,
                    ChargeOwnerId = GetChargeOwnerId(pair.Value, owners),
                    StartDateTime = pair.Key.GetStartDateTime(meteringPoint.EffectiveDate),
                    EndDateTime = pair.Key.EndDateTime,
                    OperationId = Guid.NewGuid().ToString(), // When creating default charge links, the TSO starts a new operation, which is why a new OperationId is provided.
                    Factor = DefaultChargeLink.Factor,
                })
                .ToList();

            return await CreateChargeLinksCommandAsync(createDefaultChargeLinksRequest, systemOperator, chargeLinks);
        }

        private static string GetChargeOwnerId(Charge charge, IReadOnlyCollection<MarketParticipant> owners)
        {
            return owners.Single(o => o.Id == charge.OwnerId).MarketParticipantId;
        }

        private async Task<ChargeLinksCommand> CreateChargeLinksCommandAsync(
            CreateDefaultChargeLinksRequest createDefaultChargeLinksRequest,
            MarketParticipant systemOperator,
            List<ChargeLinkDto> chargeLinks)
        {
            var currentTime = _clock.GetCurrentInstant();
            var meteringPointAdministrator = await _marketParticipantRepository.GetMeteringPointAdministratorAsync();

            return new ChargeLinksCommand(
                createDefaultChargeLinksRequest.MeteringPointId,
                new DocumentDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = DocumentType.RequestChangeBillingMasterData,
                    IndustryClassification = IndustryClassification.Electricity,
                    BusinessReasonCode = BusinessReasonCode.UpdateMasterDataSettlement,
                    RequestDate = currentTime,
                    CreatedDateTime = currentTime,
                    Sender = new MarketParticipantDto
                    {
                        Id = systemOperator.MarketParticipantId, // For default charge links the owner is the TSO.
                        BusinessProcessRole = systemOperator.BusinessProcessRole,
                    },
                    Recipient = new MarketParticipantDto
                    {
                        Id = meteringPointAdministrator.MarketParticipantId,
                        BusinessProcessRole = meteringPointAdministrator.BusinessProcessRole,
                    },
                },
                chargeLinks);
        }
    }
}
