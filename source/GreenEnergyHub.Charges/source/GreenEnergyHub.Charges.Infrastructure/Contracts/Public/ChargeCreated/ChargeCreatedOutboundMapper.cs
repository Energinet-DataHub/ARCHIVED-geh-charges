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

using Energinet.DataHub.Core.Messaging.Protobuf;
using Google.Protobuf;
using GreenEnergyHub.Charges.Application.ChargeInformation.Acknowledgement;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Core.Enumeration;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Public.ChargeCreated
{
    public class ChargeCreatedOutboundMapper : ProtobufOutboundMapper<ChargeInformationCreatedEvent>
    {
        protected override IMessage Convert(ChargeInformationCreatedEvent chargeInformationCreatedEvent)
        {
            return new Infrastructure.Integration.ChargeCreated.ChargeCreated
            {
                ChargeId = chargeInformationCreatedEvent.ChargeId,
                ChargeType = chargeInformationCreatedEvent.ChargeType.Cast<Infrastructure.Integration.ChargeCreated.ChargeCreated.Types.ChargeType>(),
                ChargeOwner = chargeInformationCreatedEvent.ChargeOwner,
                Currency = chargeInformationCreatedEvent.Currency,
                Resolution = chargeInformationCreatedEvent.Resolution.Cast<Infrastructure.Integration.ChargeCreated.ChargeCreated.Types.Resolution>(),
                TaxIndicator = chargeInformationCreatedEvent.TaxIndicator,
                StartDateTime = chargeInformationCreatedEvent.StartDateTime.ToTimestamp(),
                EndDateTime = chargeInformationCreatedEvent.EndDateTime.ToTimestamp(),
            };
        }
    }
}
