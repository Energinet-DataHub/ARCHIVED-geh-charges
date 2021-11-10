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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.CreateLinkRequest;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargeLinkCommands
{
    public class ChargeLinkCommandFactory : IChargeLinkCommandFactory
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IClock _clock;

        public ChargeLinkCommandFactory(
            IChargeRepository chargeRepository,
            IMeteringPointRepository meteringPointRepository,
            IClock clock)
        {
            _chargeRepository = chargeRepository;
            _clock = clock;
            _meteringPointRepository = meteringPointRepository;
        }

        public async Task<ChargeLinkCommand> CreateAsync(
            [NotNull] CreateLinkCommandEvent createLinkCommandEvent,
            [NotNull] DefaultChargeLink defaultChargeLink)
        {
            var charge = await _chargeRepository.GetChargeAsync(defaultChargeLink.ChargeId).ConfigureAwait(false);
            var mp = await _meteringPointRepository.GetMeteringPointAsync(
                    createLinkCommandEvent.MeteringPointId)
                .ConfigureAwait(false);

            return CreateChargeLinkCommand(
                createLinkCommandEvent.MeteringPointId,
                charge,
                defaultChargeLink.GetStartDateTime(mp.EffectiveDate),
                defaultChargeLink.EndDateTime,
                DefaultChargeLink.Factor);
        }

        public async Task<ChargeLinkCommand> CreateFromChargeLinkAsync(
            [NotNull] ChargeLink chargeLink,
            [NotNull] ChargeLinkPeriodDetails chargeLinkPeriodDetails)
        {
            var charge = await _chargeRepository.GetChargeAsync(chargeLink.ChargeId).ConfigureAwait(false);
            var meteringPoint = await _meteringPointRepository
                .GetMeteringPointAsync(chargeLink.MeteringPointId)
                .ConfigureAwait(false);

            return CreateChargeLinkCommand(
                meteringPoint.MeteringPointId,
                charge,
                chargeLinkPeriodDetails.StartDateTime,
                chargeLinkPeriodDetails.EndDateTime,
                chargeLinkPeriodDetails.Factor);
        }

        private ChargeLinkCommand CreateChargeLinkCommand(
            string meteringPointId,
            Charge charge,
            Instant startDateTime,
            Instant endDateTime,
            int factor)
        {
            var currentTime = _clock.GetCurrentInstant();
            var chargeLinkCommand = new ChargeLinkCommand()
            {
                Document = new DocumentDto
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
                    SenderProvidedChargeId = charge.SenderProvidedChargeId,
                    EndDateTime = endDateTime,
                    ChargeOwner = charge.Owner,
                    MeteringPointId = meteringPointId,
                    StartDateTime = startDateTime,
                    OperationId = Guid.NewGuid().ToString(),
                    Factor = factor,
                },
            };
            return chargeLinkCommand;
        }
    }
}
