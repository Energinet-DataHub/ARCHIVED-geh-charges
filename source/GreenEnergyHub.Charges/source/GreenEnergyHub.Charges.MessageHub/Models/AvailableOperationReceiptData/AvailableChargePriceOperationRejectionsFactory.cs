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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableChargePriceOperationRejectionsFactory :
        AvailableDataFactoryBase<AvailableChargeReceiptData.AvailableChargeReceiptData, ChargePriceOperationsRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IAvailableChargePriceReceiptValidationErrorFactory _availableChargePriceReceiptValidationErrorFactory;

        public AvailableChargePriceOperationRejectionsFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableChargePriceReceiptValidationErrorFactory availableChargePriceReceiptValidationErrorFactory,
            IMarketParticipantRepository marketParticipantRepository)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargePriceReceiptValidationErrorFactory = availableChargePriceReceiptValidationErrorFactory;
        }

        public override async Task<IReadOnlyList<AvailableChargeReceiptData.AvailableChargeReceiptData>> CreateAsync(
            ChargePriceOperationsRejectedEvent input)
        {
            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            var operationOrder = 0;

            return input.Operations.Select(operationDto => new AvailableChargeReceiptData.AvailableChargeReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    BusinessReasonCode.UpdateChargePrices,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    operationDto.OperationId[..Math.Min(operationDto.OperationId.Length, 100)],
                    DocumentType.RejectRequestChangeOfPriceList, // Will be added to the HTTP MessageType header
                    operationOrder++,
                    recipient.ActorId,
                    GetReasons(input, operationDto)))
                .ToList();
        }

        private List<AvailableReceiptValidationError> GetReasons(
            ChargePriceOperationsRejectedEvent input,
            ChargePriceOperationDto operationDto)
        {
            return input
                .ValidationErrors
                .Where(ve => ve.OperationId == operationDto.OperationId || string.IsNullOrWhiteSpace(ve.OperationId))
                .Select(validationError => _availableChargePriceReceiptValidationErrorFactory
                    .Create(validationError, input.Document, operationDto))
                .ToList();
        }
    }
}
