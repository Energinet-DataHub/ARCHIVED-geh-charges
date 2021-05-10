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
using GreenEnergyHub.Charges.Domain.Messages;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using JetBrains.Annotations;

namespace GreenEnergyHub.Charges.Domain
{
    public class ChargeAcknowledgement : IOutboundMessage
    {
        public ChargeAcknowledgement(
            string correlationId,
            string receiverMRid,
            MarketParticipantRole receiverBusinessProcessRole,
            object originalTransactionReferenceMRid,
            BusinessReasonCode businessReasonCode)
        {
            CorrelationId = correlationId;
            ReceiverMRid = receiverMRid;
            ReceiverBusinessProcessRole = receiverBusinessProcessRole;
            OriginalTransactionReferenceMRid = originalTransactionReferenceMRid;
            BusinessReasonCode = businessReasonCode;
            Transaction = Transaction.NewTransaction();
        }

        public string CorrelationId { get; }

        [UsedImplicitly]
        public string ReceiverMRid { get; }

        [UsedImplicitly]
        public MarketParticipantRole ReceiverBusinessProcessRole { get; }

        [UsedImplicitly]
        public object OriginalTransactionReferenceMRid { get; }

        [UsedImplicitly]
        public BusinessReasonCode BusinessReasonCode { get; }

        [UsedImplicitly]
        public Transaction Transaction { get; set; }
    }
}
