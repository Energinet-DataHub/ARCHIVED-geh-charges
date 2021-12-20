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
using Energinet.DataHub.Core.Messaging.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCreatedEvents;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Public.ChargeLinkCreated
{
    public class ChargeLinkCreatedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCreatedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCreatedEvent obj)
        {
            return new ChargeLinkCreatedContract
            {
                ChargeLinkId = obj.ChargeLinkId,
                MeteringPointId = obj.MeteringPointId,
                ChargeId = obj.ChargeId,
                ChargeType = (ChargeTypeContract)obj.ChargeType,
                ChargeOwner = obj.ChargeOwner,
                ChargeLinkPeriod = new ChargeLinkPeriodContract
                {
                    StartDateTime = Timestamp.FromDateTime(obj.ChargeLinkPeriod.StartDateTime.ToDateTimeUtc()),
                    EndDateTime = Timestamp.FromDateTime(obj.ChargeLinkPeriod.EndDateTime.ToDateTimeUtc()),
                    Factor = obj.ChargeLinkPeriod.Factor,
                },
            };
        }
    }
}
