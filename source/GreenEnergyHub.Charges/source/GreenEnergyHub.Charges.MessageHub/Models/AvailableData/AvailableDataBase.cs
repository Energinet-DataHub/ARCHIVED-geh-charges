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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.MessageHub.Models.AvailableData
{
    /// <summary>
    /// Shared data necessary for notifying the MessageHub and
    /// later support bundles when the market participant peeks
    /// </summary>
    public abstract class AvailableDataBase
    {
        protected AvailableDataBase(
            string senderId,
            MarketParticipantRole senderRole,
            string recipientId,
            MarketParticipantRole recipientRole,
            BusinessReasonCode businessReasonCode,
            Instant requestDateTime,
            Guid availableDataReferenceId,
            DocumentType documentType,
            int operationOrder,
            Guid actorId)
        {
            Id = Guid.NewGuid();
            SenderId = senderId;
            SenderRole = senderRole;
            RecipientId = recipientId;
            RecipientRole = recipientRole;
            BusinessReasonCode = businessReasonCode;
            RequestDateTime = requestDateTime;
            AvailableDataReferenceId = availableDataReferenceId;
            DocumentType = documentType;
            OperationOrder = operationOrder;
            ActorId = actorId;
        }

        // ReSharper disable once UnusedMember.Local - needed by persistence
        protected AvailableDataBase()
        {
            SenderId = null!;
            RecipientId = null!;
        }

        /// <summary>
        /// Unique ID of this specific available data within the charge domain,
        /// ready for shipping when the market participant peeks
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The ID of the sender of this piece of data.
        /// </summary>
        public string SenderId { get; }

        public MarketParticipantRole SenderRole { get; }

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

        public DocumentType DocumentType { get; }

        public int OperationOrder { get; }

        public Guid ActorId { get; }
    }
}
