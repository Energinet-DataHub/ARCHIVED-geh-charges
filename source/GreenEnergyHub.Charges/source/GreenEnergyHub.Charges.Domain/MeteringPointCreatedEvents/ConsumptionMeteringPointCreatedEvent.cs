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

using GreenEnergyHub.Charges.Domain.Messages.Events;
using GreenEnergyHub.Messaging.MessageTypes.Common;

namespace GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents
{
    public class ConsumptionMeteringPointCreatedEvent : InboundIntegrationEvent
    {
        public ConsumptionMeteringPointCreatedEvent(
            string meteringPointId,
            string meteringPointType,
            string gridAreaId,
            string? settlementMethod,
            string meteringMethod,
            string connectionState,
            string meterReadingPeriodicity,
            string netSettlementGroup,
            string toGrid,
            string fromGrid,
            string product,
            string quantityUnit,
            string effectiveDate,
            string parentMeteringPointId)
            : base(Transaction.NewTransaction())
        {
            MeteringPointId = meteringPointId;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            SettlementMethod = settlementMethod;
            MeteringMethod = meteringMethod;
            ConnectionState = connectionState;
            MeterReadingPeriodicity = meterReadingPeriodicity;
            NetSettlementGroup = netSettlementGroup;
            ToGrid = toGrid;
            FromGrid = fromGrid;
            Product = product;
            QuantityUnit = quantityUnit;
            EffectiveDate = effectiveDate;
            ParentMeteringPointId = parentMeteringPointId;
        }

        public string MeteringPointId { get; }

        public string MeteringPointType { get; }

        public string GridAreaId { get; }

        public string? SettlementMethod { get; }

        public string MeteringMethod { get; }

        public string ConnectionState { get; }

        public string MeterReadingPeriodicity { get; }

        public string NetSettlementGroup { get; }

        public string ToGrid { get; }

        public string FromGrid { get; }

        public string Product { get; }

        public string QuantityUnit { get; }

        public string EffectiveDate { get; }

        public string ParentMeteringPointId { get; }
    }
}
