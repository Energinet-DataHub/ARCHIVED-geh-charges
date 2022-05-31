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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
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

            // TODO: Will  fail for prices
            foreach (var chargeOperationDto in input.Command.ChargeOperations
                         .Select(x => (ChargeInformationDto)x)
                         .Where(ShouldMakeDataAvailableForActiveGridProviders))
            {
                await CreateForOperationAsync(input, chargeOperationDto, result).ConfigureAwait(false);
            }

            return result;
        }

        private async Task CreateForOperationAsync(
            ChargeCommandAcceptedEvent input,
            ChargeInformationDto informationDto,
            ICollection<AvailableChargeData> result)
        {
            var activeGridAccessProviders = await _marketParticipantRepository
                .GetGridAccessProvidersAsync()
                .ConfigureAwait(false);

            foreach (var recipient in activeGridAccessProviders)
            {
                var points = informationDto.Points
                    .Select(x => new AvailableChargeDataPoint(x.Position, x.Price)).ToList();

                var sender = await GetSenderAsync().ConfigureAwait(false);
                var operationOrder = input.Command.ChargeOperations.ToList().IndexOf(informationDto);

                result.Add(new AvailableChargeData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    informationDto.ChargeId,
                    informationDto.ChargeOwner,
                    informationDto.Type,
                    informationDto.ChargeName,
                    informationDto.ChargeDescription,
                    informationDto.StartDateTime,
                    informationDto.EndDateTime.TimeOrEndDefault(),
                    informationDto.VatClassification,
                    informationDto.TaxIndicator == TaxIndicator.Tax,
                    informationDto.TransparentInvoicing == TransparentInvoicing.Transparent,
                    informationDto.Resolution,
                    DocumentType.NotifyPriceList, // Will be added to the HTTP MessageType header
                    operationOrder,
                    points));
            }
        }

        private static bool ShouldMakeDataAvailableForActiveGridProviders(ChargeInformationDto chargeInformationDto)
        {
            // We only need to notify grid providers if the charge includes tax which are the
            // only charges they do not maintain themselves
            return chargeInformationDto.TaxIndicator == TaxIndicator.Tax;
        }
    }
}
