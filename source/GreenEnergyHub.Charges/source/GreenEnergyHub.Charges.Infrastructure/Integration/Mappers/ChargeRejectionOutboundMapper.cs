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

using System;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeRejectionOutboundMapper : ProtobufOutboundMapper<ChargeCommandRejectedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeCommandRejectedEvent rejectedEvent)
        {
            if (rejectedEvent == null)
            {
                throw new ArgumentNullException(nameof(rejectedEvent));
            }

            var chargeRejection = new ChargeRejectionContract()
            {
                CorrelationId = rejectedEvent.CorrelationId,
                ReceiverMrid = rejectedEvent.Command.Document.Sender.Id,
                ReceiverMarketParticipantRole = (MarketParticipantRoleContract)rejectedEvent.Command.Document.Sender.BusinessProcessRole,
                OriginalTransactionReferenceMrid = rejectedEvent.Command.ChargeOperation.Id,
                BusinessReasonCode = (BusinessReasonCodeContract)rejectedEvent.Command.Document.BusinessReasonCode,
            };

            foreach (var reason in rejectedEvent.Reason)
            {
                chargeRejection.RejectReasons.Add(reason);
            }

            return chargeRejection;
        }
    }
}
