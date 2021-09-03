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
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using NodaTime;

namespace GreenEnergyHub.Charges.Application.ChargeLinks.Handlers
{
    public class CreateLinkCommandEventHandler : ICreateLinkCommandEventHandler
    {
        private readonly IDefaultChargeLinkRepository _defaultChargeLinkRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMessageDispatcher<ChargeLinkCommandReceivedEvent> _messageDispatcher;
        private readonly IClock _clock;

        public CreateLinkCommandEventHandler(
            IDefaultChargeLinkRepository defaultChargeLinkRepository,
            IChargeRepository chargeRepository,
            IMessageDispatcher<ChargeLinkCommandReceivedEvent> messageDispatcher,
            IClock clock)
        {
            _defaultChargeLinkRepository = defaultChargeLinkRepository;
            _chargeRepository = chargeRepository;
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
                    var chargeLinkCommand = await CreateChargeLinkCommandAsync(
                        createLinkCommandEvent,
                        correlationId,
                        defaultChargeLink).ConfigureAwait(false);

                    var chargeLinkCommandReceivedEvent = new ChargeLinkCommandReceivedEvent(
                        _clock.GetCurrentInstant(),
                        correlationId,
                        chargeLinkCommand);

                    await _messageDispatcher.DispatchAsync(chargeLinkCommandReceivedEvent).ConfigureAwait(false);
                }
            }
        }

        private async Task<ChargeLinkCommand> CreateChargeLinkCommandAsync(
            CreateLinkCommandEvent createLinkCommandEvent,
            string correlationId,
            [NotNull]DefaultChargeLink defaultChargeLink)
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
                      Id = "5790001330552", // DataHub GLN number.
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
                    Factor = 1, // Links created from default charge links are tariffs, which only can have factor 1.
                },
            };
            return chargeLinkCommand;
        }
    }
}
