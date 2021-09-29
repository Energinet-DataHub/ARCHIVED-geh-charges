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

using System.Diagnostics.CodeAnalysis;
using Google.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeCreated;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargeCreatedOutboundMapper : ProtobufOutboundMapper<Domain.Charges.Acknowledgements.ChargeCreatedEvent>
    {
        protected override IMessage Convert([NotNull] Domain.Charges.Acknowledgements.ChargeCreatedEvent chargeCreatedEvent)
        {
            return new ChargeCreated.ChargeCreated
            {
                ChargeId = chargeCreatedEvent.ChargeId,
                ChargeType = chargeCreatedEvent.ChargeType.Cast<ChargeCreated.ChargeCreated.Types.ChargeType>(),
                ChargeOwner = chargeCreatedEvent.ChargeOwner,
                Currency = chargeCreatedEvent.Currency,
                Resolution = chargeCreatedEvent.Resolution.Cast<ChargeCreated.ChargeCreated.Types.Resolution>(),
                TaxIndicator = chargeCreatedEvent.TaxIndicator,
                ChargePeriod = new ChargePeriod
                {
                    StartDateTime = chargeCreatedEvent.ChargePeriod.StartDateTime.ToTimestamp(),
                    EndDateTime = chargeCreatedEvent.ChargePeriod.EndDateTime.ToTimestamp(),
                },
            };
        }
    }
}
