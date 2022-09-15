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
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class AvailableChargeReceiptDataFactory
        : AvailableDataFactoryBase<AvailableChargeReceiptData, ChargeInformationCommandAcceptedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;

        public AvailableChargeReceiptDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            ILoggerFactory loggerFactory,
            IMarketParticipantRepository marketParticipantRepository)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _logger = loggerFactory.CreateLogger(nameof(AvailableChargeReceiptDataFactory));
        }

        public override async Task<IReadOnlyList<AvailableChargeReceiptData>> CreateAsync(ChargeInformationCommandAcceptedEvent input)
        {
            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.Command.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            var availableChargeReceiptData = new List<AvailableChargeReceiptData>();

            var operationOrder = 0;
            foreach (var chargeOperationDto in input.Command.Operations)
            {
                availableChargeReceiptData.AddRange(CreateAvailableChargeReceiptData(
                    input.Command.Document, chargeOperationDto, sender, recipient, operationOrder++));
            }

            return availableChargeReceiptData;
        }

        private IReadOnlyList<AvailableChargeReceiptData> CreateAvailableChargeReceiptData(
            DocumentDto documentDto,
            ChargeInformationOperationDto chargeInformationOperationDto,
            MarketParticipant sender,
            MarketParticipant recipient,
            int operationOrder)
        {
            _logger.LogDebug("Recipient.Actor = {ActorId}", recipient.ActorId);
            _logger.LogDebug("Recipient.Id = {Id}", recipient.Id);
            _logger.LogDebug("Recipient.B2CActorId = {B2CActorId}", recipient.B2CActorId);
            _logger.LogDebug("Recipient.Gln = {MarketParticipantId}", recipient.MarketParticipantId);
            return new List<AvailableChargeReceiptData>()
            {
                new AvailableChargeReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    documentDto.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Confirmed,
                    chargeInformationOperationDto.OperationId[..Math.Min(chargeInformationOperationDto.OperationId.Length, 100)],
                    DocumentType.AcceptRequestChangeOfPriceList, // Will be added to the HTTP MessageType header
                    operationOrder,
                    recipient.ActorId,
                    new List<AvailableReceiptValidationError>()),
            };
        }
    }
}
