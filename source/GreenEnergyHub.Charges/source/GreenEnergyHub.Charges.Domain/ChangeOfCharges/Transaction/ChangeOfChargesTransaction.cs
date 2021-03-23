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

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    public class ChangeOfChargesTransaction
    {
        public MktActivityRecord? MktActivityRecord { get; set; }

        public string? Type { get; set; }

        [JsonPropertyName("ChargeType_mRID")]
        public string? ChargeTypeMRid { get; set; }

        [JsonPropertyName("ChargeTypeOwner_mRID")]
        public string? ChargeTypeOwnerMRid { get; set; }

        public ChargeTypePeriod? Period { get; set; }

        /// <summary>
        ///     The date this request was made.
        /// </summary>
        public Instant RequestDate { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public string? CorrelationId { get; set; }

        public string? LastUpdatedBy { get; set; }
    }
}
