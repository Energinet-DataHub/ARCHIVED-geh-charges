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
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.Collections;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeRejection;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargeRejectionInboundMapper : ProtobufInboundMapper<ChargeRejectionContract>
    {
        protected override IInboundMessage Convert([NotNull]ChargeRejectionContract rejectionContract)
        {
            return new Application.Charges.Acknowledgement.ChargeRejection(
                rejectionContract.Receiver,
                (MarketParticipantRole)rejectionContract.ReceiverMarketParticipantRole,
                rejectionContract.OriginalTransactionReference,
                (BusinessReasonCode)rejectionContract.BusinessReasonCode,
                ConvertRejectionReasons(rejectionContract.RejectReasons));
        }

        private static List<string> ConvertRejectionReasons(RepeatedField<string> rejectionReasons)
        {
            var reasons = new List<string>();
            reasons.AddRange(rejectionReasons);
            return reasons;
        }
    }
}
