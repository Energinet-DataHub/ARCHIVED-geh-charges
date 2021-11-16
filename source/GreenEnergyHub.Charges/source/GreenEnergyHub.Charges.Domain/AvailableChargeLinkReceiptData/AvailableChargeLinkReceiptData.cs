﻿// Copyright 2020 Energinet DataHub A/S
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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData
{
    public class AvailableChargeLinkReceiptData
    {
        public AvailableChargeLinkReceiptData(
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            ReceiptStatus receiptStatus,
            string originalOperationId,
            string meteringPointId,
            Instant requestTime,
            Guid availableDataReferenceId)
        {
            Id = Guid.NewGuid();
            RecipientId = recipientId;
            RecipientRole = recipientRole;
            BusinessReasonCode = businessReasonCode;
            ReceiptStatus = receiptStatus;
            OriginalOperationId = originalOperationId;
            MeteringPointId = meteringPointId;
            RequestTime = requestTime;
            AvailableDataReferenceId = availableDataReferenceId;
            _reasonCodes = new List<AvailableChargeLinkReceiptDataReasonCode>();
        }

        public Guid Id { get; }

        public string RecipientId { get; }

        public MarketParticipantRole RecipientRole { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public ReceiptStatus ReceiptStatus { get; }

        public string OriginalOperationId { get; }

        public string MeteringPointId { get; }

        private readonly List<AvailableChargeLinkReceiptDataReasonCode> _reasonCodes;

        public IReadOnlyCollection<AvailableChargeLinkReceiptDataReasonCode> Points => _reasonCodes.AsReadOnly();

        public Instant RequestTime { get; }

        public Guid AvailableDataReferenceId { get; }
    }
}
