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

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.External.MeteringPointCreated
{
    public class MeteringPointCreatedInboundMapper : ProtobufInboundMapper<Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated>
    {
        public static SettlementMethod? MapSettlementMethod(Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            switch (settlementMethod)
            {
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.SettlementMethod.SmFlex:
                    return SettlementMethod.Flex;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.SettlementMethod.SmProfiled:
                    return SettlementMethod.Profiled;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.SettlementMethod.SmNonprofiled:
                    return SettlementMethod.NonProfiled;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.SettlementMethod.SmUnknown:
                    return null;
                default:
                    throw new InvalidEnumArgumentException($"Provided SettlementMethod value '{settlementMethod}' is invalid and cannot be mapped.");
            }
        }

        public static ConnectionState MapConnectionState(Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.ConnectionState connectionState)
        {
            switch (connectionState)
            {
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.ConnectionState.CsNew:
                    return ConnectionState.New;
                default:
                    throw new InvalidEnumArgumentException($"Provided ConnectionState value '{connectionState}' is invalid and cannot be mapped.");
            }
        }

        public static MeteringPointType MapMeteringPointType(
            Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType
                meteringPointType)
        {
            switch (meteringPointType)
            {
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptAnalysis:
                    return MeteringPointType.Analysis;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptConsumption:
                    return MeteringPointType.Consumption;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptExchange:
                    return MeteringPointType.Exchange;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptProduction:
                    return MeteringPointType.Unknown;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptVeproduction:
                    return MeteringPointType.VeProduction;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptElectricalHeating:
                    return MeteringPointType.ElectricalHeating;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptInternalUse:
                    return MeteringPointType.InternalUse;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptNetConsumption:
                    return MeteringPointType.NetConsumption;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptNetProduction:
                    return MeteringPointType.NetProduction;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptOtherConsumption:
                    return MeteringPointType.OtherConsumption;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptOtherProduction:
                    return MeteringPointType.OtherProduction;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptOwnProduction:
                    return MeteringPointType.OwnProduction;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptTotalConsumption:
                    return MeteringPointType.TotalConsumption;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptWholesaleServices:
                    return MeteringPointType.WholesaleService;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptConsumptionFromGrid:
                    return MeteringPointType.ConsumptionFromGrid;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptExchangeReactiveEnergy:
                    return MeteringPointType.ExchangeReactiveEnergy;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptGridLossCorrection:
                    return MeteringPointType.GridLossCorrection;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptNetFromGrid:
                    return MeteringPointType.NetFromGrid;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptNetToGrid:
                    return MeteringPointType.NetToGrid;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptSupplyToGrid:
                    return MeteringPointType.SupplyToGrid;
                case Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types.MeteringPointType.MptSurplusProductionGroup:
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
