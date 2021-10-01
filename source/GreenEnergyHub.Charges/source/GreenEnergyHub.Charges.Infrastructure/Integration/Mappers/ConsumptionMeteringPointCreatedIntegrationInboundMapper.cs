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
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ConsumptionMeteringPointCreatedIntegrationInboundMapper : ProtobufInboundMapper<ConsumptionMeteringPointCreated>
    {
        protected override IInboundMessage Convert([NotNull] ConsumptionMeteringPointCreated obj)
        {
            var settlementMethod = MapSettlementMethod(obj.SettlementMethod);
            var meterMethod = MapMeterMethod(obj.MeteringMethod);
            var meterReadingPeriodicity = MapMeterReadingPeriodicity(obj.MeterReadingPeriodicity);
            var netSettlementMethod = MapNetSettlementMethod(obj.NetSettlementGroup);
            var productType = MapProductType(obj.Product);

            return new ConsumptionMeteringPointCreatedEvent(
                obj.MeteringPointId,
                obj.GsrnNumber,
                obj.GridAreaCode,
                settlementMethod,
                meterMethod,
                meterReadingPeriodicity,
                netSettlementMethod,
                productType,
                obj.EffectiveDate);
        }

        private static SettlementMethod MapSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
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

        private static MeteringMethod MapMeterMethod(ConsumptionMeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            switch (meteringMethod)
            {
                case ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical:
                    return MeteringMethod.Physical;
                case ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual:
                    return MeteringMethod.Virtual;
                case ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated:
                    return MeteringMethod.Calculated;
                default:
                    return MeteringMethod.Unknown;
            }
        }

        private static MeterReadingPeriodicity MapMeterReadingPeriodicity(ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            switch (meterReadingPeriodicity)
            {
                case ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpYearly:
                    return MeterReadingPeriodicity.Yearly;
                case ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpMonthly:
                    return MeterReadingPeriodicity.Monthly;
                case ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly:
                    return MeterReadingPeriodicity.Hourly;
                case ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly:
                    return MeterReadingPeriodicity.Quarterly;
                default:
                    return MeterReadingPeriodicity.Unknown;
            }
        }

        private static NetSettlementGroup MapNetSettlementMethod(ConsumptionMeteringPointCreated.Types.NetSettlementGroup netSettlementGroup)
        {
            switch (netSettlementGroup)
            {
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgZero:
                    return NetSettlementGroup.Zero;
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgOne:
                    return NetSettlementGroup.One;
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgTwo:
                    return NetSettlementGroup.Two;
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgThree:
                    return NetSettlementGroup.Three;
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgSix:
                    return NetSettlementGroup.Six;
                case ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine:
                    return NetSettlementGroup.NinetyNine;
                default:
                    return NetSettlementGroup.Unknown;
            }
        }

        private static ProductType MapProductType(ConsumptionMeteringPointCreated.Types.ProductType product)
        {
            switch (product)
            {
                case ConsumptionMeteringPointCreated.Types.ProductType.PtTariff:
                    return ProductType.Tariff;
                case ConsumptionMeteringPointCreated.Types.ProductType.PtFuelquantity:
                    return ProductType.FuelQuantity;
                case ConsumptionMeteringPointCreated.Types.ProductType.PtPoweractive:
                    return ProductType.PowerActive;
                case ConsumptionMeteringPointCreated.Types.ProductType.PtPowerreactive:
                    return ProductType.PowerReActive;
                case ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive:
                    return ProductType.EnergyActive;
                case ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyreactive:
                    return ProductType.EnergyReActive;
                default:
                    return ProductType.Unknown;
            }
        }
    }
}
