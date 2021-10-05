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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;

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
                    return SettlementMethod.Unknown;
            }
        }

        protected override IInboundMessage Convert([NotNull] ConsumptionMeteringPointCreated obj)
        {
            var settlementMethod = MapSettlementMethod(obj.SettlementMethod);

            return new ConsumptionMeteringPointCreatedEvent(
                obj.GsrnNumber,
                obj.GridAreaCode,
                settlementMethod,
                obj.EffectiveDate.ToInstant());
        }
    }
}
