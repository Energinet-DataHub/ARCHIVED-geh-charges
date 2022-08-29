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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableChargePriceOperationConfirmationsFactory :
        AvailableDataFactoryBase<AvailableChargeReceiptData.AvailableChargeReceiptData, PriceConfirmedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;

        public AvailableChargePriceOperationConfirmationsFactory(
            IMessageMetaDataContext messageMetaDataContext,
            ILoggerFactory loggerFactory,
            IMarketParticipantRepository marketParticipantRepository)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _logger = loggerFactory.CreateLogger(nameof(AvailableChargePriceOperationConfirmationsFactory));
        }

        public override async Task<IReadOnlyList<AvailableChargeReceiptData.AvailableChargeReceiptData>> CreateAsync(
            PriceConfirmedEvent input)
        {
            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            var availableChargeReceiptData = new List<AvailableChargeReceiptData.AvailableChargeReceiptData>();

            var operationOrder = 0;
            foreach (var chargePriceOperationDto in input.Operations)
            {
                availableChargeReceiptData.AddRange(CreateAvailableChargeReceiptData(
                    input.Document, chargePriceOperationDto, sender, recipient, operationOrder++));
            }

            return availableChargeReceiptData;
        }

        private IReadOnlyList<AvailableChargeReceiptData.AvailableChargeReceiptData> CreateAvailableChargeReceiptData(
            DocumentDto documentDto,
            ChargePriceOperationDto chargePriceOperationDto,
            MarketParticipant sender,
            MarketParticipant recipient,
            int operationOrder)
        {
            _logger.LogDebug("Recipient.Actor = {ActorId}", recipient.ActorId);
            _logger.LogDebug("Recipient.Id = {Id}", recipient.Id);
            _logger.LogDebug("Recipient.B2CActorId = {B2CActorId}", recipient.B2CActorId);
            _logger.LogDebug("Recipient.Gln = {MarketParticipantId}", recipient.MarketParticipantId);
            return new List<AvailableChargeReceiptData.AvailableChargeReceiptData>()
            {
                new AvailableChargeReceiptData.AvailableChargeReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    documentDto.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Confirmed,
                    chargePriceOperationDto.OperationId[..Math.Min(chargePriceOperationDto.OperationId.Length, 100)],
                    DocumentType.ConfirmRequestChangeOfPriceList, // Will be added to the HTTP MessageType header
                    operationOrder,
                    recipient.ActorId,
                    new List<AvailableReceiptValidationError>()),
            };
        }
    }
}
