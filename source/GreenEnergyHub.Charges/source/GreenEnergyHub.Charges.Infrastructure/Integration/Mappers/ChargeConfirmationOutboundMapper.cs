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

using System;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeConfirmation;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargeConfirmationOutboundMapper : ProtobufOutboundMapper<ChargeCommandAcceptedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeCommandAcceptedEvent obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return new ChargeConfirmationContract
            {
                CorrelationId = obj.CorrelationId,
                ReceiverMrid = obj.Command.Document.Sender.Id,
                ReceiverMarketParticipantRole = (MarketParticipantRoleContract)obj.Command.Document.Sender.BusinessProcessRole,
                OriginalTransactionReferenceMrid = obj.Command.ChargeOperation.Id,
                BusinessReasonCode = (BusinessReasonCodeContract)obj.Command.Document.BusinessReasonCode,
            };
        }
    }
}
