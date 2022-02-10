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

using GreenEnergyHub.Charges.Domain.Charges;
using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands
{
    public class ChargeLinkDto
    {
        /// <summary>
        /// Contains a ID for the specific link, provided by the sender (or TSO when creating default charge link). Combined with sender.id it becomes unique.
        /// </summary>
        public string OperationId { get; set; }

        public Instant StartDateTime { get; set; }

        public Instant? EndDateTime { get; set; }

        public string SenderProvidedChargeId { get; set; }

        public int Factor { get; set; }

        public string ChargeOwner { get; set; }

        public ChargeType ChargeType { get; set; }
    }
}
