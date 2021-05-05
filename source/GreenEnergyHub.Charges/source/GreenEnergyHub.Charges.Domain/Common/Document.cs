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

using System.Text.Json.Serialization;
using NodaTime;
#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.Common
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // MarketDocument integrity is null checked by ChargeCommandNullChecker
    public class Document
    {
        public string Id { get; set; }

        public Instant CreatedDateTime { get; set; }

        [JsonPropertyName("Sender_MarketParticipant")]
        public MarketParticipant Sender { get; set; }

        [JsonPropertyName("Receiver_MarketParticipant")]
        public MarketParticipant Recipient { get; set; }

        [JsonPropertyName("Market_ServiceCategoryKind")]
        public IndustryClassification IndustryClassification { get; set; }

        public BusinessReasonCode BusinessReasonCode { get; set; }
    }
}
