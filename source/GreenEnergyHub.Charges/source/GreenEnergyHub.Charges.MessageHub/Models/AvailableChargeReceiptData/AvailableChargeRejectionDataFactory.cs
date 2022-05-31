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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData
{
    public class AvailableChargeRejectionDataFactory :
        AvailableDataFactoryBase<AvailableChargeReceiptData, ChargeCommandRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly IAvailableChargeReceiptValidationErrorFactory _availableChargeReceiptValidationErrorFactory;
        private readonly ILogger _logger;

        public AvailableChargeRejectionDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableChargeReceiptValidationErrorFactory availableChargeReceiptValidationErrorFactory,
            IMarketParticipantRepository marketParticipantRepository,
            ILoggerFactory loggerFactory)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeReceiptValidationErrorFactory = availableChargeReceiptValidationErrorFactory;
            _logger = loggerFactory.CreateLogger(nameof(AvailableChargeRejectionDataFactory));
        }

        public override async Task<IReadOnlyList<AvailableChargeReceiptData>> CreateAsync(ChargeCommandRejectedEvent input)
        {
            LogValidationErrors(input);

            var recipient = input.Command.Document.Sender; // The original sender is the recipient of the receipt
            var sender = await GetSenderAsync().ConfigureAwait(false);

            var operationOrder = 0;

            return input.Command.ChargeOperations.Select(chargeOperationDto => new AvailableChargeReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.Id,
                    recipient.BusinessProcessRole,
                    input.Command.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    chargeOperationDto.Id,
                    DocumentType.RejectRequestChangeOfPriceList, // Will be added to the HTTP MessageType header
                    operationOrder++,
                    GetReasons(input, chargeOperationDto)))
                .ToList();
        }

        private void LogValidationErrors(ChargeCommandRejectedEvent rejectedEvent)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                rejectedEvent.Command.Document,
                rejectedEvent.ValidationErrors);

            _logger.LogError("ValidationErrors for {errorMessage}", errorMessage);
        }

        private List<AvailableReceiptValidationError> GetReasons(
            ChargeCommandRejectedEvent input,
            ChargeInformationDto chargeInformationDto)
        {
            return input
                .ValidationErrors
                .Where(ve => ve.OperationId == chargeInformationDto.Id || string.IsNullOrWhiteSpace(ve.OperationId))
                .Select(validationError => _availableChargeReceiptValidationErrorFactory
                    .Create(validationError, input.Command, chargeInformationDto))
                .ToList();
        }
    }
}
