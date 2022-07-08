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
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands
{
    public class ChargeLinkDto : OperationBase
    {
        public ChargeLinkDto(
            string operationId,
            string meteringPointId,
            Instant startDateTime,
            Instant? endDateTime,
            string senderProvidedChargeId,
            int factor,
            string chargeOwner,
            ChargeType chargeType)
            : base(chargeOwner)
        {
            OperationId = operationId;
            MeteringPointId = meteringPointId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            SenderProvidedChargeId = senderProvidedChargeId;
            Factor = factor;
            ChargeType = chargeType;
        }

        /// <summary>
        /// Contains a ID for the specific link, provided by the sender (or TSO when creating default charge link).
        /// Combined with sender.id it becomes unique.
        /// </summary>
        public string OperationId { get; }

        public string MeteringPointId { get; }

        public Instant StartDateTime { get; }

        public Instant? EndDateTime { get; set; }

        public string SenderProvidedChargeId { get; }

        public int Factor { get; }

        public ChargeType ChargeType { get; }
    }
}
