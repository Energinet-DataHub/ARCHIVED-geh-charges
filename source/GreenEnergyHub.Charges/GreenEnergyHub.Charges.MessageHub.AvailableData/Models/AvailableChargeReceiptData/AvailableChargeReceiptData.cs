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

using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.AvailableData.Models.AvailableChargeReceiptData
{
    /// <summary>
    /// All data necessary for one activity records in a charge link receipt
    /// The data will be stored on events and will later be fetched as part
    /// of creating a bundle of receipts for a market participant once the
    /// participant peek the MessageHub
    /// </summary>
    public class AvailableChargeReceiptData : AvailableDataBase
    {
        public AvailableChargeReceiptData(
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            ReceiptStatus receiptStatus,
            string originalOperationId,
            DocumentType documentType,
            int operationOrder,
            Guid actorId,
            List<AvailableReceiptValidationError> validationErrors)
            : base(
                senderId,
                senderRole,
                recipientId,
                recipientRole,
                businessReasonCode,
                requestDateTime,
                availableDataReferenceId,
                documentType,
                operationOrder,
                actorId)
        {
            ReceiptStatus = receiptStatus;
            OriginalOperationId = originalOperationId;
            _validationErrors = validationErrors;
        }

        // ReSharper disable once UnusedMember.Local - Used implicitly by persistence
        private AvailableChargeReceiptData()
        {
            OriginalOperationId = null!;
            _validationErrors = new List<AvailableReceiptValidationError>();
        }

        public ReceiptStatus ReceiptStatus { get; }

        public string OriginalOperationId { get; }

        private readonly List<AvailableReceiptValidationError> _validationErrors;

        public IReadOnlyCollection<AvailableReceiptValidationError> ValidationErrors => _validationErrors.AsReadOnly();
    }
}
