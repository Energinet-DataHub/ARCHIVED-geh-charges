﻿// Copyright 2020 Energinet DataHub A/S
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

using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Domain.Messages;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Domain.Acknowledgements
{
    public class ChargeConfirmation : IMessage, IOutboundMessage
    {
        public ChargeConfirmation(
            string correlationId,
            string receiverMRid,
            MarketParticipantRole receiverMarketParticipantRole,
            string originalTransactionReferenceMRid,
            BusinessReasonCode businessReasonCode)
        {
            CorrelationId = correlationId;
            ReceiverMRid = receiverMRid;
            ReceiverMarketParticipantRole = receiverMarketParticipantRole;
            OriginalTransactionReferenceMRid = originalTransactionReferenceMRid;
            BusinessReasonCode = businessReasonCode;
            Transaction = Transaction.NewTransaction();
        }

        public string CorrelationId { get; }

        public string ReceiverMRid { get; }

        public MarketParticipantRole ReceiverMarketParticipantRole { get; }

        public string OriginalTransactionReferenceMRid { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public Transaction Transaction { get; set; }
    }
}
