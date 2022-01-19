﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Infrastructure.Contracts.External.MeteringPointCreated;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using Xunit;
using Xunit.Categories;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Contracts.External.MeteringPointCreated
{
    [UnitTest]
    public class MeteringPointCreatedIntegrationInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void MeteringPointCreatedIntegrationInboundMapper_WhenCalled_ShouldMapToProtobufWithCorrectValues(
            [NotNull] Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated meteringPointCreatedEvent,
            [NotNull] MeteringPointCreatedInboundMapper sut)
        {
            meteringPointCreatedEvent.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));
            meteringPointCreatedEvent.MeteringPointType = mpTypes.MeteringPointType.MptConsumption;
            meteringPointCreatedEvent.SettlementMethod = mpTypes.SettlementMethod.SmFlex;
            meteringPointCreatedEvent.ConnectionState = mpTypes.ConnectionState.CsNew;

            // Act
            var converted = (MeteringPointCreatedEvent)sut.Convert(meteringPointCreatedEvent);

            // Assert
            converted.Should().NotContainNullsOrEmptyEnumerables();
            converted.MeteringPointId.Should().Be(meteringPointCreatedEvent.GsrnNumber);
            converted.EffectiveDate.Should().Be(meteringPointCreatedEvent.EffectiveDate.ToInstant());
            converted.GridAreaId.Should().Be(meteringPointCreatedEvent.GridAreaCode);
            converted.SettlementMethod.Should().Be(SettlementMethod.Flex);
            converted.ConnectionState.Should().Be(ConnectionState.New);
            converted.MeteringPointType.Should().Be(MeteringPointType.Consumption);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalledWithNull_ShouldThrow([NotNull] MeteringPointCreatedInboundMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null!));
        }

        [Theory]
        [InlineData(mpTypes.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(mpTypes.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(mpTypes.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        [InlineData(mpTypes.SettlementMethod.SmNull, null)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(mpTypes.SettlementMethod protoSettlementMethod, SettlementMethod? expectedSettlementMethod)
        {
            var actual =
                MeteringPointCreatedInboundMapper.MapSettlementMethod(protoSettlementMethod);

            actual.Should().Be(expectedSettlementMethod);
        }

        [Fact]
        public void MapSettlementMethod_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => MeteringPointCreatedInboundMapper.MapSettlementMethod((mpTypes.SettlementMethod)9999));
        }

        [Theory]
        [InlineData(mpTypes.ConnectionState.CsNew, ConnectionState.New)]
        public void MapConnectionState_WhenCalled_ShouldMapCorrectly(
            mpTypes.ConnectionState protoConnectionState,
            ConnectionState expectedConnectionState)
        {
            var actual = MeteringPointCreatedInboundMapper.MapConnectionState(protoConnectionState);

            actual.Should().Be(expectedConnectionState);
        }

        [Fact]
        public void MapConnectionState_WhenCalledWithInvalidEnum_Throws()
        {
            Assert.Throws<InvalidEnumArgumentException>(
                () => MeteringPointCreatedInboundMapper.MapConnectionState((mpTypes.ConnectionState)9999));
        }

        [Theory]
        [InlineData(mpTypes.MeteringPointType.MptAnalysis, MeteringPointType.Analysis)]
        [InlineData(mpTypes.MeteringPointType.MptConsumption, MeteringPointType.Consumption)]
        [InlineData(mpTypes.MeteringPointType.MptConsumptionFromGrid, MeteringPointType.ConsumptionFromGrid)]
        [InlineData(mpTypes.MeteringPointType.MptElectricalHeating, MeteringPointType.ElectricalHeating)]
        [InlineData(mpTypes.MeteringPointType.MptExchange, MeteringPointType.Exchange)]
        [InlineData(mpTypes.MeteringPointType.MptExchangeReactiveEnergy, MeteringPointType.ExchangeReactiveEnergy)]
        [InlineData(mpTypes.MeteringPointType.MptInternalUse, MeteringPointType.InternalUse)]
        [InlineData(mpTypes.MeteringPointType.MptNetConsumption, MeteringPointType.NetConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptNetFromGrid, MeteringPointType.NetFromGrid)]
        [InlineData(mpTypes.MeteringPointType.MptNetProduction, MeteringPointType.NetProduction)]
        [InlineData(mpTypes.MeteringPointType.MptNetToGrid, MeteringPointType.NetToGrid)]
        [InlineData(mpTypes.MeteringPointType.MptOtherConsumption, MeteringPointType.OtherConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptOtherProduction, MeteringPointType.OtherProduction)]
        [InlineData(mpTypes.MeteringPointType.MptOwnProduction, MeteringPointType.OwnProduction)]
        [InlineData(mpTypes.MeteringPointType.MptProduction, MeteringPointType.Production)]
        [InlineData(mpTypes.MeteringPointType.MptSupplyToGrid, MeteringPointType.SupplyToGrid)]
        [InlineData(mpTypes.MeteringPointType.MptSurplusProductionGroup, MeteringPointType.SurplusProductionGroup)]
        [InlineData(mpTypes.MeteringPointType.MptTotalConsumption, MeteringPointType.TotalConsumption)]
        [InlineData(mpTypes.MeteringPointType.MptVeproduction, MeteringPointType.VeProduction)]
        [InlineData(mpTypes.MeteringPointType.MptWholesaleServices, MeteringPointType.WholesaleService)]
        public void MapMeteringPointType_WhenCalled_ShouldMapCorrectly(
            mpTypes.MeteringPointType protoMeteringType,
            MeteringPointType expectedMeteringPointType)
        {
            var actual =
                MeteringPointCreatedInboundMapper.MapMeteringPointType(protoMeteringType);

            actual.Should().Be(expectedMeteringPointType);
        }
    }
}
