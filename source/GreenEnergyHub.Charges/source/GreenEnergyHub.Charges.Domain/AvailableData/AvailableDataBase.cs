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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.AvailableData
{
    /// <summary>
    /// Shared data necessary for notifying the MessageHub and
    /// later support bundles when the market participant peeks
    /// </summary>
    public abstract class AvailableDataBase
    {
        public AvailableDataBase(
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId)
        {
            Id = Guid.NewGuid();
            RecipientId = recipientId;
            RecipientRole = recipientRole;
            BusinessReasonCode = businessReasonCode;
            RequestDateTime = requestDateTime;
            AvailableDataReferenceId = availableDataReferenceId;
        }

        public AvailableDataBase(string recipientId)
        {
            RecipientId = recipientId;
        }

        /// <summary>
        /// Unique ID of this specific available data within the charge domain,
        /// ready for shipping when the market participant peeks
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The ID of the recipient this piece of data is meant for
        /// </summary>
        public string RecipientId { get; }

        public MarketParticipantRole RecipientRole { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public Instant RequestDateTime { get; }

        /// <summary>
        /// ID of the data used when notifying the MessageHub.
        /// The ID will later be used to fetch the data on a peek operation for the MessageHub
        /// </summary>
        public Guid AvailableDataReferenceId { get; }
    }
}
