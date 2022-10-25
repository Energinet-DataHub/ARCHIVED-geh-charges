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
using System.ComponentModel.DataAnnotations;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    /// <summary>
    /// Class is used for handling messages related to a charge.
    /// </summary>
    public class ChargeMessage
    {
        private ChargeMessage(
            string senderProvidedChargeId,
            ChargeType type,
            string marketParticipantId,
            string messageId,
            DocumentType documentType,
            Instant documentRequestDate)
        {
            Id = Guid.NewGuid();
            SenderProvidedChargeId = senderProvidedChargeId;
            Type = type;
            MarketParticipantId = marketParticipantId;
            MessageId = messageId;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private ChargeMessage()
        {
            SenderProvidedChargeId = string.Empty;
            MarketParticipantId = string.Empty;
            MessageId = string.Empty;
        }

        /// <summary>
        /// Globally unique identifier of the charge message reference.
        /// </summary>
        public Guid Id { get; }

        [Required]
        [StringLength(35)]
        public string SenderProvidedChargeId { get; }

        public ChargeType Type { get; }

        [Required]
        [StringLength(35)]
        public string MarketParticipantId { get; }

        /// <summary>
        /// The message id
        /// </summary>
        public string MessageId { get; }

        public static ChargeMessage Create(
            string senderProvidedChargeId,
            ChargeType chargeType,
            string marketParticipantId,
            string messageId,
            DocumentType documentType,
            Instant documentRequestDate)
        {
            ArgumentNullException.ThrowIfNull(senderProvidedChargeId);
            ArgumentNullException.ThrowIfNull(chargeType);
            ArgumentNullException.ThrowIfNull(marketParticipantId);
            ArgumentNullException.ThrowIfNull(messageId);
            ArgumentNullException.ThrowIfNull(documentType);
            ArgumentNullException.ThrowIfNull(documentRequestDate);

            return new ChargeMessage(
                senderProvidedChargeId,
                chargeType,
                marketParticipantId,
                messageId,
                documentType,
                documentRequestDate);
        }
    }
}
