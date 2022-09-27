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

using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.Dtos.SharedDtos
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // Document integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// The Document class contains relevant information from the Business Message in which the ChargeEvent(s) were delivered to the Charges domain.
    /// This includes among other things sender and recipient details, An ID provided by the sender, and some business context related data.
    /// </summary>
    public class DocumentDto
    {
        public DocumentDto(string id, Instant requestDate, DocumentType type, Instant createdDateTime, MarketParticipantDto sender, MarketParticipantDto recipient, IndustryClassification industryClassification, BusinessReasonCode businessReasonCode)
        {
            Id = id;
            RequestDate = requestDate;
            Type = type;
            CreatedDateTime = createdDateTime;
            Sender = sender;
            Recipient = recipient;
            IndustryClassification = industryClassification;
            BusinessReasonCode = businessReasonCode;
        }

        /// <summary>
        /// An ID provided by the sender.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///  Point in time set by the Charges domain
        /// </summary>
        public Instant RequestDate { get; }

        public DocumentType Type { get; }

        /// <summary>
        /// A point in time provided by the sender
        /// </summary>
        public Instant CreatedDateTime { get; }

        public MarketParticipantDto Sender { get; }

        public MarketParticipantDto Recipient { get; }

        public IndustryClassification IndustryClassification { get; }

        public BusinessReasonCode BusinessReasonCode { get; }
    }
}
