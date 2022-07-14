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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData
{
    public class AvailableChargeDataFactory : AvailableDataFactoryBase<AvailableChargeData, ChargeCommandAcceptedEvent>
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public AvailableChargeDataFactory(
            IMarketParticipantRepository marketParticipantRepository,
            IMessageMetaDataContext messageMetaDataContext)
            : base(marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public override async Task<IReadOnlyList<AvailableChargeData>> CreateAsync(ChargeCommandAcceptedEvent input)
        {
            var result = new List<AvailableChargeData>();

            foreach (var chargeOperationDto in
                     input.Command.Operations.Where(x =>
                         ShouldMakeDataAvailableForActiveGridProviders(x, input.Command.Document)))
            {
                await CreateForOperationAsync(input, chargeOperationDto, result).ConfigureAwait(false);
            }

            return result;
        }

        private async Task CreateForOperationAsync(
            ChargeCommandAcceptedEvent input,
            ChargeOperationDto operation,
            ICollection<AvailableChargeData> result)
        {
            var activeGridAccessProviders = await _marketParticipantRepository
                .GetGridAccessProvidersAsync()
                .ConfigureAwait(false);

            foreach (var recipient in activeGridAccessProviders)
            {
                var points = operation.Points
                    .Select(x => new AvailableChargeDataPoint(x.Position, x.Price)).ToList();

                var sender = await GetSenderAsync().ConfigureAwait(false);
                var operationOrder = input.Command.Operations.ToList().IndexOf(operation);

                result.Add(new AvailableChargeData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    operation.ChargeId,
                    operation.ChargeOwner,
                    operation.Type,
                    operation.ChargeName,
                    operation.ChargeDescription,
                    operation.StartDateTime,
                    operation.EndDateTime.TimeOrEndDefault(),
                    operation.VatClassification,
                    operation.TaxIndicator == TaxIndicator.Tax,
                    operation.TransparentInvoicing == TransparentInvoicing.Transparent,
                    operation.Resolution,
                    DocumentType.NotifyPriceList, // Will be added to the HTTP MessageType header
                    operationOrder,
                    recipient.ActorId,
                    points));
            }
        }

        private static bool ShouldMakeDataAvailableForActiveGridProviders(
            ChargeOperationDto chargeOperationDto, DocumentDto documentDto)
        {
            // Todo: Remove when Price handling is separated in its own flow:
            // Issue: 1411: New Price Flow: #3 Accept/reject/notify and persist
            var isRelevantForPriceFlow =
                documentDto.BusinessReasonCode == BusinessReasonCode.UpdateChargePrices
                && chargeOperationDto.Type == ChargeType.Tariff;

            // We only need to notify grid providers if the charge includes tax which are the
            // only charges they do not maintain themselves
            return chargeOperationDto.TaxIndicator == TaxIndicator.Tax || isRelevantForPriceFlow;
        }
    }
}
