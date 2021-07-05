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
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class MeteringPointCreatedIntegrationInboundMapper : ProtobufInboundMapper<MeteringPointCreated>
    {
        protected override IInboundMessage Convert(MeteringPointCreated obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return new MeteringPointCreatedEvent(
                obj.MeteringPointId,
                obj.MeteringPointType,
                obj.MeteringGridArea,
                obj.SettlementMethod,
                obj.MeteringMethod,
                obj.ConnectionState,
                obj.MeterReadingPeriodicity,
                obj.NetSettlementGroup,
                obj.ToGrid,
                obj.FromGrid,
                obj.Product,
                obj.QuantityUnit,
                obj.EffectiveDate)
            {
                MeteringPointId = obj.MeteringPointId,
                EffectiveDate = obj.EffectiveDate,
            };
        }
    }
}
