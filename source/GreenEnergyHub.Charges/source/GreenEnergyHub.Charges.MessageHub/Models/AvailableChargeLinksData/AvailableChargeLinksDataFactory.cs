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
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData
{
    public class AvailableChargeLinksDataFactory
        : AvailableDataFactoryBase<AvailableChargeLinksData, ChargeLinksAcceptedEvent>
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public AvailableChargeLinksDataFactory(
            IMarketParticipantRepository marketParticipantRepository,
            IChargeRepository chargeRepository,
            IChargeIdentifierFactory chargeIdentifierFactory,
            IMessageMetaDataContext messageMetaDataContext)
            : base(marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _chargeRepository = chargeRepository;
            _chargeIdentifierFactory = chargeIdentifierFactory;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public override async Task<IReadOnlyList<AvailableChargeLinksData>> CreateAsync(ChargeLinksAcceptedEvent input)
        {
            var result = new List<AvailableChargeLinksData>();

            foreach (var chargeLinksOperation in input.Command.Operations)
            {
                await CreateForOperationsAsync(input, chargeLinksOperation, result).ConfigureAwait(false);
            }

            return result;
        }

        private async Task CreateForOperationsAsync(
            ChargeLinksAcceptedEvent input,
            ChargeLinkOperationDto operation,
            ICollection<AvailableChargeLinksData> result)
        {
            // It is the responsibility of the Charge Domain to find the recipient and
            // not considered part of the Create Metering Point orchestration.
            // We select the first as all bundled messages will have the same recipient
            var recipient = await _marketParticipantRepository
                .GetGridAccessProviderAsync(operation.MeteringPointId)
                .ConfigureAwait(false);

            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(operation.SenderProvidedChargeId, operation.ChargeType, operation.ChargeOwner)
                .ConfigureAwait(false);

            var charge = await _chargeRepository.SingleAsync(chargeIdentifier).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);
            if (!ShouldMakeDataAvailableForGridOwnerOfMeteringPoint(charge)) return;
            var operationOrder = input.Command.Operations.ToList().IndexOf(operation);
            result.Add(new AvailableChargeLinksData(
                sender.MarketParticipantId,
                sender.BusinessProcessRole,
                recipient.MarketParticipantId,
                recipient.BusinessProcessRole,
                input.Command.Document.BusinessReasonCode,
                _messageMetaDataContext.RequestDataTime,
                Guid.NewGuid(), // ID of each available piece of data must be unique
                operation.SenderProvidedChargeId,
                operation.ChargeOwner,
                operation.ChargeType,
                operation.MeteringPointId,
                operation.Factor,
                operation.StartDateTime,
                operation.EndDateTime.GetValueOrDefault(),
                DocumentType.NotifyBillingMasterData, // Will be added to the HTTP MessageType header
                operationOrder,
                recipient.ActorId));
        }

        private static bool ShouldMakeDataAvailableForGridOwnerOfMeteringPoint(Charge charge)
        {
            // We only need to notify the grid provider owning the metering point if
            // the link is about a tax charge; the rest they maintain themselves
            return charge.TaxIndicator;
        }
    }
}
