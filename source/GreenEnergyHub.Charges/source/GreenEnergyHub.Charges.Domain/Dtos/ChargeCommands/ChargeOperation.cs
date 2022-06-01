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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands
{
    public class ChargeOperation : OperationBase
    {
        [SuppressMessage("Usage", "MemberCanBeProtected.Global", Justification = "Must be public to enable deserialization.")]
        public ChargeOperation(string id, string chargeId, ChargeType type, string chargeOwner, Instant startDateTime)
        {
            Id = id;
            ChargeId = chargeId;
            Type = type;
            ChargeOwner = chargeOwner;
            StartDateTime = startDateTime;
        }

        /// <summary>
        /// Contains a unique ID for the specific Charge OperationId, provided by the sender.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants).
        /// Example: EA-001
        /// </summary>
        public string ChargeId { get; }

        /// <summary>
        /// Charge Type, subscription, fee or tariff
        /// </summary>
        public ChargeType Type { get; }

        /// <summary>
        ///  Charge Owner, e.g. the GLN or EIC identification number.
        /// </summary>
        public string ChargeOwner { get; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; }
    }
}
