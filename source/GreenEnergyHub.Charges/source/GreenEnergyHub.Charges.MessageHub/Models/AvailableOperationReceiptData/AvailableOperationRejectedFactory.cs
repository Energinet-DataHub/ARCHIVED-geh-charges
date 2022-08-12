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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableOperationReceiptData
{
    public class AvailableOperationRejectionsFactory :
        AvailableDataFactoryBase<AvailableChargeReceiptData.AvailableChargeReceiptData, OperationsRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IAvailableOperationReceiptValidationErrorFactory _availableChargeReceiptValidationErrorFactory;
        private readonly ILogger _logger;

        public AvailableOperationRejectionsFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableOperationReceiptValidationErrorFactory availableChargeReceiptValidationErrorFactory,
            IMarketParticipantRepository marketParticipantRepository,
            ILoggerFactory loggerFactory)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeReceiptValidationErrorFactory = availableChargeReceiptValidationErrorFactory;
            _logger = loggerFactory.CreateLogger(nameof(AvailableChargeRejectionDataFactory));
        }

        public override async Task<IReadOnlyList<AvailableChargeReceiptData.AvailableChargeReceiptData>> CreateAsync(OperationsRejectedEvent input)
        {
            LogValidationErrors(input);

            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.Command.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            var operationOrder = 0;

            return input.Command.Operations.Select(operationDto => new AvailableChargeReceiptData.AvailableChargeReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    operationDto.Id[..Math.Min(operationDto.Id.Length, 100)],
                    DocumentType.RejectRequestChangeOfPriceList, // Will be added to the HTTP MessageType header
                    operationOrder++,
                    recipient.ActorId,
                    GetReasons(input, operationDto)))
                .ToList();
        }

        private void LogValidationErrors(OperationsRejectedEvent rejectedEvent)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                rejectedEvent.Command.Document,
                rejectedEvent.ValidationErrors);
            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);
        }

        private List<AvailableReceiptValidationError> GetReasons(
            OperationsRejectedEvent input,
            ChargePriceOperationDto operationDto)
        {
            return input
                .ValidationErrors
                .Where(ve => ve.OperationId == operationDto.Id || string.IsNullOrWhiteSpace(ve.OperationId))
                .Select(validationError => _availableChargeReceiptValidationErrorFactory
                    .Create(validationError, input.Command, operationDto))
                .ToList();
        }
    }
}
