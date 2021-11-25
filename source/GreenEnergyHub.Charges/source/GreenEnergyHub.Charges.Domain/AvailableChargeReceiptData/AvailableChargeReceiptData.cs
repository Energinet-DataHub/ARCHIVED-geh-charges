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
using GreenEnergyHub.Charges.Domain.AvailableData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData
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
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            ReceiptStatus receiptStatus,
            string originalOperationId,
            List<AvailableChargeReceiptDataReasonCode> reasonCodes)
            : base(recipientId, recipientRole, businessReasonCode, requestDateTime, availableDataReferenceId)
        {
            ReceiptStatus = receiptStatus;
            OriginalOperationId = originalOperationId;
            _reasonCodes = reasonCodes;
        }

        /// <summary>
        /// Used implicitly by persistence.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private AvailableChargeReceiptData(string recipientId, string originalOperationId)
            : base(recipientId)
        {
            OriginalOperationId = originalOperationId;
            _reasonCodes = new List<AvailableChargeReceiptDataReasonCode>();
        }

        public ReceiptStatus ReceiptStatus { get; }

        public string OriginalOperationId { get; }

        private readonly List<AvailableChargeReceiptDataReasonCode> _reasonCodes;

        public IReadOnlyCollection<AvailableChargeReceiptDataReasonCode> ReasonCodes => _reasonCodes.AsReadOnly();
    }
}
