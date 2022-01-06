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

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.External.ConsumptionMeteringPointCreated
{
    public class ConsumptionMeteringPointCreatedInboundMapper : ProtobufInboundMapper<MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated>
    {
        public static SettlementMethod MapSettlementMethod(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            switch (settlementMethod)
            {
                case MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex:
                    return SettlementMethod.Flex;
                case MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled:
                    return SettlementMethod.Profiled;
                case MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled:
                    return SettlementMethod.NonProfiled;
                default:
                    throw new InvalidEnumArgumentException($"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped.");
            }
        }

        public static ConnectionState MapConnectionState(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew:
                    return ConnectionState.New;
                default:
                    throw new InvalidEnumArgumentException($"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped.");
            }
        }

        protected override IInboundMessage Convert([NotNull] MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated consumptionMeteringPointCreated)
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
