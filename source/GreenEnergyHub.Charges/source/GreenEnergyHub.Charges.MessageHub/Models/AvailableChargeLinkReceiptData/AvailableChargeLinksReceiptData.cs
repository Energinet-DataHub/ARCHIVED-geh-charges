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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData
{
    /// <summary>
    /// All data necessary for one activity records in a charge link receipt
    /// The data will be stored on events and will later be fetched as part
    /// of creating a bundle of receipts for a market participant once the
    /// participant peek the MessageHub
    /// </summary>
    public class AvailableChargeLinksReceiptData : AvailableDataBase
    {
        public AvailableChargeLinksReceiptData(
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            ReceiptStatus receiptStatus,
            string originalOperationId,
            string meteringPointId,
            List<AvailableReceiptValidationError> validationErrors)
            : base(recipientId, recipientRole, businessReasonCode, requestDateTime, availableDataReferenceId)
        {
            ReceiptStatus = receiptStatus;
            OriginalOperationId = originalOperationId;
            MeteringPointId = meteringPointId;
            _validationErrors = validationErrors;
        }

        /// <summary>
        /// Used implicitly by persistence.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private AvailableChargeLinksReceiptData(string recipientId, string originalOperationId, string meteringPointId)
            : base(recipientId)
        {
            OriginalOperationId = originalOperationId;
            MeteringPointId = meteringPointId;
            _validationErrors = new List<AvailableReceiptValidationError>();
        }

        public ReceiptStatus ReceiptStatus { get; }

        public string OriginalOperationId { get; }

        public string MeteringPointId { get; }

        private readonly List<AvailableReceiptValidationError> _validationErrors;

        public IReadOnlyCollection<AvailableReceiptValidationError> ValidationErrors => _validationErrors.AsReadOnly();
    }
}
