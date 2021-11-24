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

using System.Collections.Generic;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using GreenEnergyHub.Charges.Domain.Dtos.Messages;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.Charges.Acknowledgement
{
    public class ChargeRejection : IMessage
    {
        public ChargeRejection(
            string receiver,
            MarketParticipantRole receiverMarketParticipantRole,
            string originalTransactionReference,
            BusinessReasonCode businessReasonCode,
            IEnumerable<string> rejectReasons)
        {
            Receiver = receiver;
            ReceiverMarketParticipantRole = receiverMarketParticipantRole;
            OriginalTransactionReference = originalTransactionReference;
            BusinessReasonCode = businessReasonCode;
            RejectReasons = rejectReasons;
            Transaction = Transaction.NewTransaction();
        }

        public string Receiver { get; }

        public MarketParticipantRole ReceiverMarketParticipantRole { get; }

        public string OriginalTransactionReference { get; }

        public BusinessReasonCode BusinessReasonCode { get; }

        public IEnumerable<string> RejectReasons { get; }

        public Transaction Transaction { get; set; }
    }
}
