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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Contracts.External.ConsumptionMeteringPointCreated;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.External.ConsumptionMeteringPointCreated
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedIntegrationInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void ConsumptionMeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull] MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated meteringPointCreatedEvent,
            [NotNull] ConsumptionMeteringPointCreatedInboundMapper sut)
        {
            meteringPointCreatedEvent.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var converted = (ConsumptionMeteringPointCreatedEvent)sut.Convert(meteringPointCreatedEvent);

            // Assert
            converted.Should().NotContainNullsOrEmptyEnumerables();
            converted.MeteringPointId.Should().Be(meteringPointCreatedEvent.GsrnNumber);
            converted.EffectiveDate.Should().Be(meteringPointCreatedEvent.EffectiveDate.ToInstant());
            converted.GridAreaId.Should().Be(meteringPointCreatedEvent.GridAreaCode);
            converted.SettlementMethod.Should().NotBe(SettlementMethod.Unknown);
            converted.ConnectionState.Should().NotBe(ConnectionState.Unknown);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull]ConsumptionMeteringPointCreatedInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        [Theory]
        [InlineData(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod protoSettlementMethod, SettlementMethod expectedSettlementMethod)
        {
            var actual =
                ConsumptionMeteringPointCreatedInboundMapper.MapSettlementMethod(protoSettlementMethod);

            actual.Should().Be(expectedSettlementMethod);
        }

        [Fact]
        public void MapSettlementMethod_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => ConsumptionMeteringPointCreatedInboundMapper.MapSettlementMethod((MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.SettlementMethod)9999));
        }

        [Theory]
        [InlineData(MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew, ConnectionState.New)]
        public void MapConnectionState_WhenCalled_ShouldMapCorrectly(
            MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.ConnectionState protoConnectionState,
            ConnectionState expectedConnectionState)
        {
            var actual = ConsumptionMeteringPointCreatedInboundMapper.MapConnectionState(protoConnectionState);

            actual.Should().Be(expectedConnectionState);
        }

        [Fact]
        public void MapConnectionState_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => ConsumptionMeteringPointCreatedInboundMapper.MapConnectionState((MeteringPoints.IntegrationEventContracts.ConsumptionMeteringPointCreated.Types.ConnectionState)9999));
        }
    }
}
