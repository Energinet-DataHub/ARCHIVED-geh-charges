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

using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    public class ChargeLink : InboundIntegrationEvent, IOutboundMessage
    {
        public ChargeLink()
            : base(Messaging.MessageTypes.Common.Transaction.NewTransaction())
        { }

        /// <summary>
        /// Contains a unique ID for the specific link, provided by the sender.
        /// </summary>
        public string Id { get; set; }

        public string MeteringPointId { get; set; }

        public Instant StartDateTime { get; set; }

        public Instant? EndDateTime { get; set; }

        public string ChargeId { get; set; }

        public int Factor { get; set; }

        public string ChargeOwner { get; set; }

        public ChargeType ChargeType { get; set; }
    }
}
