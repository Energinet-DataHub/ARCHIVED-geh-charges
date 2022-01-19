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
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.External.MeteringPointCreated
{
    public class MeteringPointCreatedInboundMapper : ProtobufInboundMapper<Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated>
    {
        public static SettlementMethod? MapSettlementMethod(mpTypes.SettlementMethod settlementMethod)
        {
            switch (settlementMethod)
            {
                case mpTypes.SettlementMethod.SmFlex:
                    return SettlementMethod.Flex;
                case mpTypes.SettlementMethod.SmProfiled:
                    return SettlementMethod.Profiled;
                case mpTypes.SettlementMethod.SmNonprofiled:
                    return SettlementMethod.NonProfiled;
                case mpTypes.SettlementMethod.SmNull:
                    return null;
                default:
                    throw new InvalidEnumArgumentException($"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped.");
            }
        }

        public static ConnectionState MapConnectionState(mpTypes.ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case mpTypes.ConnectionState.CsNew:
                    return ConnectionState.New;
                default:
                    throw new InvalidEnumArgumentException($"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped.");
            }
        }

        public static MeteringPointType MapMeteringPointType(
            mpTypes.MeteringPointType
                meteringPointType)
        {
            switch (meteringPointType)
            {
                case mpTypes.MeteringPointType.MptAnalysis:
                    return MeteringPointType.Analysis;
                case mpTypes.MeteringPointType.MptConsumption:
                    return MeteringPointType.Consumption;
                case mpTypes.MeteringPointType.MptExchange:
                    return MeteringPointType.Exchange;
                case mpTypes.MeteringPointType.MptProduction:
                    return MeteringPointType.Unknown;
                case mpTypes.MeteringPointType.MptVeproduction:
                    return MeteringPointType.VeProduction;
                case mpTypes.MeteringPointType.MptElectricalHeating:
                    return MeteringPointType.ElectricalHeating;
                case mpTypes.MeteringPointType.MptInternalUse:
                    return MeteringPointType.InternalUse;
                case mpTypes.MeteringPointType.MptNetConsumption:
                    return MeteringPointType.NetConsumption;
                case mpTypes.MeteringPointType.MptNetProduction:
                    return MeteringPointType.NetProduction;
                case mpTypes.MeteringPointType.MptOtherConsumption:
                    return MeteringPointType.OtherConsumption;
                case mpTypes.MeteringPointType.MptOtherProduction:
                    return MeteringPointType.OtherProduction;
                case mpTypes.MeteringPointType.MptOwnProduction:
                    return MeteringPointType.OwnProduction;
                case mpTypes.MeteringPointType.MptTotalConsumption:
                    return MeteringPointType.TotalConsumption;
                case mpTypes.MeteringPointType.MptWholesaleServices:
                    return MeteringPointType.WholesaleService;
                case mpTypes.MeteringPointType.MptConsumptionFromGrid:
                    return MeteringPointType.ConsumptionFromGrid;
                case mpTypes.MeteringPointType.MptExchangeReactiveEnergy:
                    return MeteringPointType.ExchangeReactiveEnergy;
                case mpTypes.MeteringPointType.MptGridLossCorrection:
                    return MeteringPointType.GridLossCorrection;
                case mpTypes.MeteringPointType.MptNetFromGrid:
                    return MeteringPointType.NetFromGrid;
                case mpTypes.MeteringPointType.MptNetToGrid:
                    return MeteringPointType.NetToGrid;
                case mpTypes.MeteringPointType.MptSupplyToGrid:
                    return MeteringPointType.SupplyToGrid;
                case mpTypes.MeteringPointType.MptSurplusProductionGroup:
                    return MeteringPointType.SurplusProductionGroup;
                default:
                    throw new InvalidEnumArgumentException($"Provided MeteringPointType value '{meteringPointType}' is invalid and cannot be mapped.");
            }
        }

        protected override IInboundMessage Convert([NotNull] Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated meteringPointCreated)
        {
            var settlementMethod = MapSettlementMethod(meteringPointCreated.SettlementMethod);
            var connectionState = MapConnectionState(meteringPointCreated.ConnectionState);
            var meteringPointType = MapMeteringPointType(meteringPointCreated.MeteringPointType);

            return new MeteringPointCreatedEvent(
                meteringPointCreated.GsrnNumber,
                meteringPointCreated.GridAreaCode,
                settlementMethod,
                connectionState,
                meteringPointCreated.EffectiveDate.ToInstant(),
                meteringPointType);
        }
    }
}
