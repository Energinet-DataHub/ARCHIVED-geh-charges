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

using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableData;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.Shared;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeLinksReceiptData
{
    public class AvailableChargeLinksRejectionDataFactory
        : AvailableDataFactoryBase<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeLinksReceiptData.AvailableChargeLinksReceiptData, ChargeLinksRejectedEvent>
    {
        private readonly IMessageMetaDataContext _messageMetaDataContext;
        private readonly ILogger _logger;

        private readonly IAvailableChargeLinksReceiptValidationErrorFactory
            _availableChargeLinksReceiptValidationErrorFactory;

        public AvailableChargeLinksRejectionDataFactory(
            IMessageMetaDataContext messageMetaDataContext,
            IAvailableChargeLinksReceiptValidationErrorFactory availableChargeLinksReceiptValidationErrorFactory,
            IMarketParticipantRepository marketParticipantRepository,
            ILoggerFactory loggerFactory)
            : base(marketParticipantRepository)
        {
            _messageMetaDataContext = messageMetaDataContext;
            _availableChargeLinksReceiptValidationErrorFactory = availableChargeLinksReceiptValidationErrorFactory;
            _logger = loggerFactory.CreateLogger(nameof(AvailableChargeLinksRejectionDataFactory));
        }

        public override async Task<IReadOnlyList<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeLinksReceiptData.AvailableChargeLinksReceiptData>> CreateAsync(
            ChargeLinksRejectedEvent input)
        {
            LogValidationErrors(input);

            if (AvailableDataFactoryHelper.ShouldSkipAvailableData(input.ChargeLinksCommand))
                return new List<GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeLinksReceiptData.AvailableChargeLinksReceiptData>();

            // The original sender is the recipient of the receipt
            var recipient = await GetRecipientAsync(input.ChargeLinksCommand.Document.Sender).ConfigureAwait(false);
            var sender = await GetSenderAsync().ConfigureAwait(false);

            return input.ChargeLinksCommand.Operations.Select(chargeLinkDto =>
                new GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeLinksReceiptData.AvailableChargeLinksReceiptData(
                    sender.MarketParticipantId,
                    sender.BusinessProcessRole,
                    recipient.MarketParticipantId,
                    recipient.BusinessProcessRole,
                    input.ChargeLinksCommand.Document.BusinessReasonCode,
                    _messageMetaDataContext.RequestDataTime,
                    Guid.NewGuid(), // ID of each available piece of data must be unique
                    ReceiptStatus.Rejected,
                    chargeLinkDto.OperationId[..Math.Min(chargeLinkDto.OperationId.Length, 100)],
                    chargeLinkDto.MeteringPointId,
                    DocumentType.RejectRequestChangeBillingMasterData, // Will be added to the HTTP MessageType header
                    input.ChargeLinksCommand.Operations.ToList().IndexOf(chargeLinkDto),
                    recipient.ActorId,
                    GetReasons(input, chargeLinkDto)))
                .ToList();
        }

        private void LogValidationErrors(ChargeLinksRejectedEvent rejectedEvent)
        {
            var errorMessage = ValidationErrorLogMessageBuilder.BuildErrorMessage(
                rejectedEvent.ChargeLinksCommand.Document,
                rejectedEvent.ValidationErrors);

            _logger.LogError("ValidationErrors for {ErrorMessage}", errorMessage);
        }

        private List<AvailableReceiptValidationError> GetReasons(
            ChargeLinksRejectedEvent input,
            ChargeLinkDto chargeLinkDto)
        {
            return input
                .ValidationErrors
                .Where(ve => ve.OperationId == chargeLinkDto.OperationId || string.IsNullOrWhiteSpace(ve.OperationId))
                .Select(validationError => _availableChargeLinksReceiptValidationErrorFactory
                    .Create(validationError, input.ChargeLinksCommand, chargeLinkDto))
                .ToList();
        }
    }
}
