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

using GreenEnergyHub.Charges.Domain.Common;
using JetBrains.Annotations;

namespace GreenEnergyHub.Charges.Domain
{
    public class ChargeAcknowledgement
    {
        public ChargeAcknowledgement(string correlationId, string senderMRid, MarketParticipantRole senderBusinessProcessRole, string eventId, object originalTransactionReferenceMRid, ProcessType businessReasonCode)
        {
            CorrelationId = correlationId;
            SenderMRid = senderMRid;
            SenderBusinessProcessRole = senderBusinessProcessRole;
            EventId = eventId;
            OriginalTransactionReferenceMRid = originalTransactionReferenceMRid;
            BusinessReasonCode = businessReasonCode;
        }

        public string CorrelationId { get; }

        [UsedImplicitly]
        public string SenderMRid { get; }

        [UsedImplicitly]
        public MarketParticipantRole SenderBusinessProcessRole { get; }

        [UsedImplicitly]
        public string EventId { get; }

        [UsedImplicitly]
        public object OriginalTransactionReferenceMRid { get; }

        [UsedImplicitly]
        public ProcessType BusinessReasonCode { get; }
    }
}
