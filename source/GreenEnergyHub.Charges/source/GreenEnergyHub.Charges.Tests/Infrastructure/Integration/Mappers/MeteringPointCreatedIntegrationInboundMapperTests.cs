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

using System;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class MeteringPointCreatedIntegrationInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void MeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull] MeteringPointCreated meteringPointCreatedEvent,
            [NotNull] MeteringPointCreatedIntegrationInboundMapper sut)
        {
            // Act
            var converted = (MeteringPointCreatedEvent)sut.Convert(meteringPointCreatedEvent);

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

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]MeteringPointCreatedIntegrationInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }
    }
}
