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

using GreenEnergyHub.Charges.Domain.Command;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NodaTime;
using MarketDocument = GreenEnergyHub.Charges.Domain.Common.MarketDocument;
#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeCommand integrity is null checked by ChargeCommandNullChecker
    public class ChargeCommand : CommandBase
    {
        public ChargeCommand([NotNull] string correlationId)
            : base(correlationId)
        {
        }

        public MarketDocument MarketDocument { get; set; }

        public MktActivityRecord MktActivityRecord { get; set; }

        /// <summary>
        /// The kind of charge: Fee ("D02") | Subscription ("D01") | Tariff ("D03").
        /// </summary>
        public string Type { get; set; }

        [JsonProperty("ChargeType_mRID")]
        public string ChargeTypeMRid { get; set; }

        [JsonProperty("ChargeTypeOwner_mRID")]
        public string ChargeTypeOwnerMRid { get; set; }

        public ChargeTypePeriod Period { get; set; }

        /// <summary>
        ///     The date this request was made.
        /// </summary>
        public Instant RequestDate { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public string LastUpdatedBy { get; set; }
    }
}
