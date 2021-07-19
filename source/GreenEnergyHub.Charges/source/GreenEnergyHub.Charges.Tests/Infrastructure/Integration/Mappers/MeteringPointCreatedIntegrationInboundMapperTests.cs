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

using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class MeteringPointCreatedIntegrationInboundMapperTests
    {
        [Fact]
        public void MeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToProtobufWithCorrectValues()
        {
            // Arrange
            var meteringPointCreatedEvent = new MeteringPointCreated
            {
                Product = "1",
                ConnectionState = "2",
                EffectiveDate = "3",
                FromGrid = "4",
                MeteringMethod = "5",
                QuantityUnit = "6",
                SettlementMethod = "7",
                ToGrid = "8",
                MeteringGridArea = "9",
                MeteringPointId = "10",
                MeteringPointType = "11",
                MeterReadingPeriodicity = "12",
                NetSettlementGroup = "13",
                ParentMeteringPointId = "14",
            };

            var mapper = new MeteringPointCreatedIntegrationInboundMapper();

            // Act
            var converted = (MeteringPointCreatedEvent)mapper.Convert(meteringPointCreatedEvent);

            // Assert
            converted.Should().NotContainNullsOrEmptyEnumerables();
            converted.ConnectionState.Should().BeEquivalentTo(meteringPointCreatedEvent.ConnectionState);
            converted.EffectiveDate.Should().BeEquivalentTo(meteringPointCreatedEvent.EffectiveDate);
            converted.FromGrid.Should().BeEquivalentTo(meteringPointCreatedEvent.FromGrid);
            converted.MeteringMethod.Should().BeEquivalentTo(meteringPointCreatedEvent.MeteringMethod);
            converted.QuantityUnit.Should().BeEquivalentTo(meteringPointCreatedEvent.QuantityUnit);
            converted.SettlementMethod.Should().BeEquivalentTo(meteringPointCreatedEvent.SettlementMethod);
            converted.ToGrid.Should().BeEquivalentTo(meteringPointCreatedEvent.ToGrid);
            //converted.MeteringGridArea.Should().BeEquivalentTo(meteringPointCreatedEvent.MeteringGridArea);
            converted.MeteringPointId.Should().BeEquivalentTo(meteringPointCreatedEvent.MeteringPointId);
            converted.MeteringPointType.Should().BeEquivalentTo(meteringPointCreatedEvent.MeteringPointType);
            converted.MeterReadingPeriodicity.Should().BeEquivalentTo(meteringPointCreatedEvent.MeterReadingPeriodicity);
            converted.NetSettlementGroup.Should().BeEquivalentTo(meteringPointCreatedEvent.NetSettlementGroup);
            //converted.ParentMeteringPointId.Should().BeEquivalentTo(meteringPointCreatedEvent.ParentMeteringPointId);
        }
    }
}
