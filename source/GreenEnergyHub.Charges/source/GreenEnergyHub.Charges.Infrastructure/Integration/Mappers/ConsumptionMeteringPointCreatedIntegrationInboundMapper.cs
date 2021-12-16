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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.MeteringPoints.IntegrationEventContracts;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ConsumptionMeteringPointCreatedIntegrationInboundMapper : ProtobufInboundMapper<ConsumptionMeteringPointCreated>
    {
        public static SettlementMethod MapSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            switch (settlementMethod)
            {
                case ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex:
                    return SettlementMethod.Flex;
                case ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled:
                    return SettlementMethod.Profiled;
                case ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled:
                    return SettlementMethod.NonProfiled;
                default:
                    throw new InvalidEnumArgumentException($"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped.");
            }
        }

        public static ConnectionState MapConnectionState(ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew:
                    return ConnectionState.New;
                default:
                    throw new InvalidEnumArgumentException($"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped.");
            }
        }

        protected override IInboundMessage Convert([NotNull] ConsumptionMeteringPointCreated consumptionMeteringPointCreated)
        {
            var settlementMethod = MapSettlementMethod(consumptionMeteringPointCreated.SettlementMethod);
            var connectionState = MapConnectionState(consumptionMeteringPointCreated.ConnectionState);

            return new ConsumptionMeteringPointCreatedEvent(
                consumptionMeteringPointCreated.GsrnNumber,
                consumptionMeteringPointCreated.GridAreaCode,
                settlementMethod,
                connectionState,
                consumptionMeteringPointCreated.EffectiveDate.ToInstant());
        }
    }
}
