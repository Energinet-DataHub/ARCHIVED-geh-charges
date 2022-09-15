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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData
{
    public class AvailableChargePriceDataFactory : AvailableDataFactoryBase<AvailableChargePriceData, ChargePriceOperationsConfirmedEvent>
    {
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IChargeRepository _chargeRepository;
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        public AvailableChargePriceDataFactory(
            IChargeIdentifierFactory chargeIdentifierFactory,
            IChargeRepository chargeRepository,
            IMarketParticipantRepository marketParticipantRepository,
            IMessageMetaDataContext messageMetaDataContext)
            : base(marketParticipantRepository)
        {
            _chargeIdentifierFactory = chargeIdentifierFactory;
            _chargeRepository = chargeRepository;
            _marketParticipantRepository = marketParticipantRepository;
            _messageMetaDataContext = messageMetaDataContext;
        }

        public override async Task<IReadOnlyList<AvailableChargePriceData>> CreateAsync(ChargePriceOperationsConfirmedEvent input)
        {
            var result = new List<AvailableChargePriceData>();

            var firstChargePriceOperation = input.Operations.First();
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(
                    firstChargePriceOperation.SenderProvidedChargeId,
                    firstChargePriceOperation.ChargeType,
                    firstChargePriceOperation.ChargeOwner)
                .ConfigureAwait(false);
            var charge = await _chargeRepository.SingleAsync(chargeIdentifier).ConfigureAwait(false);
            if (!charge.TaxIndicator) return result;

            foreach (var chargePriceOperationDto in input.Operations)
            {
                await CreateForOperationAsync(input, chargePriceOperationDto, result).ConfigureAwait(false);
            }

            return result;
        }

        private async Task CreateForOperationAsync(
            ChargePriceOperationsConfirmedEvent input,
            ChargePriceOperationDto priceOperation,
            ICollection<AvailableChargePriceData> result)
        {
            var activeGridAccessProviders = await _marketParticipantRepository
                .GetGridAccessProvidersAsync()
                .ConfigureAwait(false);
            foreach (var recipient in activeGridAccessProviders)
            {
                var points = priceOperation.Points
                    .Select(point =>
                        new AvailableChargePriceDataPoint(priceOperation.Points.GetPositionOfPoint(point), point.Price))
                    .ToList();

                var sender = await GetSenderAsync().ConfigureAwait(false);
                var operationOrder = input.Operations.ToList().IndexOf(priceOperation);

                result.Add(new AvailableChargePriceData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    availableDataReferenceId: Guid.NewGuid(), // ID of each available piece of data must be unique
                    priceOperation.SenderProvidedChargeId,
                    priceOperation.ChargeOwner,
                    priceOperation.ChargeType,
                    priceOperation.StartDateTime,
                    priceOperation.Resolution,
                    DocumentType.NotifyPriceList, // Will be added to the HTTP MessageType header
                    operationOrder,
                    recipient.ActorId,
                    points));
            }
        }
    }
}
