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
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedIntegrationInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void ConsumptionMeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull] ConsumptionMeteringPointCreated meteringPointCreatedEvent,
            [NotNull] ConsumptionMeteringPointCreatedIntegrationInboundMapper sut)
        {
            // Act
            var converted = (ConsumptionMeteringPointCreatedEvent)sut.Convert(meteringPointCreatedEvent);

            // Assert
            converted.Should().NotContainNullsOrEmptyEnumerables();
            converted.MeteringPointId.Should().Be(meteringPointCreatedEvent.MeteringPointId);
            converted.EffectiveDate.Should().BeEquivalentTo(meteringPointCreatedEvent.EffectiveDate);
            converted.GridAreaCode.Should().BeEquivalentTo(meteringPointCreatedEvent.GridAreaCode);
            converted.SettlementMethod.Should().NotBe(SettlementMethod.Unknown);
            converted.MeteringMethod.Should().NotBe(MeteringMethod.Unknown);
            converted.NetSettlementGroup.Should().NotBe(NetSettlementGroup.Unknown);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ConsumptionMeteringPointCreatedIntegrationInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        [Theory]
        [InlineData(ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(ConsumptionMeteringPointCreated.Types.SettlementMethod protoSettlementMethod, SettlementMethod expectedSettlementMethod)
        {
            var actual =
                ConsumptionMeteringPointCreatedIntegrationInboundMapper.MapSettlementMethod(protoSettlementMethod);

            actual.Should().Be(expectedSettlementMethod);
        }

        [Theory]
        [InlineData(ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated, MeteringMethod.Calculated)]
        [InlineData(ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual, MeteringMethod.Virtual)]
        [InlineData(ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical, MeteringMethod.Physical)]
        public void MapMeterMethod_WhenCalled_ShouldMapCorrectly(ConsumptionMeteringPointCreated.Types.MeteringMethod protoMeterMethod, MeteringMethod expectedMeterMethod)
        {
            var actual =
                ConsumptionMeteringPointCreatedIntegrationInboundMapper.MapMeterMethod(protoMeterMethod);

            actual.Should().Be(expectedMeterMethod);
        }

        [Theory]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgZero, NetSettlementGroup.Zero)]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgOne, NetSettlementGroup.One)]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgTwo, NetSettlementGroup.Two)]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgThree, NetSettlementGroup.Three)]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgSix, NetSettlementGroup.Six)]
        [InlineData(ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine, NetSettlementGroup.NinetyNine)]
        public void MapNetSettlementGroup_WhenCalled_ShouldMapCorrectly(ConsumptionMeteringPointCreated.Types.NetSettlementGroup protoNetSettlementGroup, NetSettlementGroup expectedNetSettlementGroup)
        {
            var actual =
                ConsumptionMeteringPointCreatedIntegrationInboundMapper.MapNetSettlementMethod(protoNetSettlementGroup);

            actual.Should().Be(expectedNetSettlementGroup);
        }
    }
}
