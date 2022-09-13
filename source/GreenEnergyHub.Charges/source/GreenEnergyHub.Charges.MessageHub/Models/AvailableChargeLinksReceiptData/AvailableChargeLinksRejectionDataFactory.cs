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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData
{
    public class AvailableChargeLinksRejectionDataFactory
        : AvailableDataFactoryBase<AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;

        private readonly IAvailableChargeLinksReceiptValidationErrorFactory
            _availableChargeLinksReceiptValidationErrorFactory;

        public AvailableChargeLinksRejectionDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableChargeLinksReceiptValidationErrorFactory availableChargeLinksReceiptValidationErrorFactory,
            IMarketParticipantRepository marketParticipantRepository)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeLinksReceiptValidationErrorFactory = availableChargeLinksReceiptValidationErrorFactory;
        }

        public override async Task<IReadOnlyList<AvailableChargeLinksReceiptData>> CreateAsync(
            ChargeLinksRejectedEvent input)
        {
            if (AvailableDataFactoryHelper.ShouldSkipAvailableData(input.Command))
                return new List<AvailableChargeLinksReceiptData>();

            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.Command.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            return input.Command.Operations.Select(chargeLinkDto =>
                new AvailableChargeLinksReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    chargeLinkDto.OperationId[..Math.Min(chargeLinkDto.OperationId.Length, 100)],
                    chargeLinkDto.MeteringPointId,
                    DocumentType.RejectRequestChangeBillingMasterData, // Will be added to the HTTP MessageType header
                    input.Command.Operations.ToList().IndexOf(chargeLinkDto),
                    recipient.ActorId,
                    GetReasons(input, chargeLinkDto)))
                .ToList();
        }

        private List<AvailableReceiptValidationError> GetReasons(
            ChargeLinksRejectedEvent input,
            ChargeLinkOperationDto chargeLinkOperationDto)
        {
            return input
                .ValidationErrors
                .Where(ve => ve.OperationId == chargeLinkOperationDto.OperationId || string.IsNullOrWhiteSpace(ve.OperationId))
                .Select(validationError => _availableChargeLinksReceiptValidationErrorFactory
                    .Create(validationError, input.Command.Document, chargeLinkOperationDto))
                .ToList();
        }
    }
}
