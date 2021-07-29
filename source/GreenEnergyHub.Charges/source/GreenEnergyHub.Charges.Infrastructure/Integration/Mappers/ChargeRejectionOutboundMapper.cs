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
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargeRejectionOutboundMapper : ProtobufOutboundMapper<Domain.Acknowledgements.ChargeRejection>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]Domain.Acknowledgements.ChargeRejection rejection)
        {
            var chargeRejection = new ChargeRejectionContract
            {
                CorrelationId = rejection.CorrelationId,
                ReceiverMrid = rejection.ReceiverMRid,
                ReceiverMarketParticipantRole = (MarketParticipantRoleContract)rejection.ReceiverMarketParticipantRole,
                OriginalTransactionReferenceMrid = rejection.OriginalTransactionReferenceMRid,
                BusinessReasonCode = (BusinessReasonCodeContract)rejection.BusinessReasonCode,
            };

            foreach (var reason in rejection.RejectReason)
            {
                chargeRejection.RejectReasons.Add(reason);
            }

            return chargeRejection;
        }
    }
}
