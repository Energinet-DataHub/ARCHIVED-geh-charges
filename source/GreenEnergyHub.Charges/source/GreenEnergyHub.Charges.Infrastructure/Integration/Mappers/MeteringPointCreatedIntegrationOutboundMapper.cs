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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Transport.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class MeteringPointCreatedIntegrationOutboundMapper : ProtobufOutboundMapper<MeteringPointCreatedEvent>
    {
        protected override IMessage Convert(MeteringPointCreatedEvent obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return new MeteringPointCreated()
            {
                MeteringPointId = obj.MeteringPointId,
                EffectiveDate = obj.EffectiveDate,
                ConnectionState = obj.ConnectionState,
                FromGrid = obj.FromGrid,
                ToGrid = obj.ToGrid,
                MeteringGridArea = obj.GridAreaId,
                MeteringMethod = obj.MeteringMethod,
                MeteringPointType = obj.MeteringPointType,
                MeterReadingPeriodicity = obj.MeterReadingPeriodicity,
                NetSettlementGroup = obj.NetSettlementGroup,
                //ParentMeteringPointId = obj.Pa
                Product = obj.Product,
                QuantityUnit = obj.QuantityUnit,
                SettlementMethod = obj.SettlementMethod,
            };
        }
    }
}
