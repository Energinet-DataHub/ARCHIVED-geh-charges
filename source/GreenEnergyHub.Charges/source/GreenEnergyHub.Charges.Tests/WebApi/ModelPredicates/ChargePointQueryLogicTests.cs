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
using System.Collections.Generic;
using System.Linq;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargePrice;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using GreenEnergyHub.Iso8601;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.WebApi.ModelPredicates
{
    [UnitTest]
    public class ChargePointQueryLogicTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsPT15M_SetsAllPropertiesCorrectly(
            ChargePoint chargePoint)
        {
            // Arrange
            chargePoint.Time = new DateTime(2022, 1, 1, 1, 0, 0);
            chargePoint.Charge.Resolution = (int)Resolution.PT15M;

            var expected = new ChargePriceV1Dto(
                chargePoint.Price,
                chargePoint.Time,
                new DateTime(2022, 1, 1, 2, 15, 0));

            var chargePoints = new List<ChargePoint> { chargePoint }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.Single().Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsPT15M_TimeShouldNotOverlap(
            ChargePoint chargePoint,
            ChargePoint chargePoint2)
        {
            // Arrange
            chargePoint.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 0, 0), DateTimeKind.Utc);
            chargePoint.Charge.Resolution = (int)Resolution.PT15M;

            chargePoint2.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 6, 0), DateTimeKind.Utc);
            chargePoint2.Charge.Resolution = (int)Resolution.PT15M;

            var expected = new List<ChargePriceV1Dto>
            {
                new(
                    chargePoint.Price,
                    chargePoint.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 6, 0), DateTimeKind.Utc)),
                new(
                    chargePoint2.Price,
                    chargePoint2.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 15, 0), DateTimeKind.Utc)),
            };

            var chargePoints = new List<ChargePoint> { chargePoint, chargePoint2 }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsP1M_DatesShouldNotOverlap(
            ChargePoint chargePoint,
            ChargePoint chargePoint2,
            ChargePoint chargePoint3)
        {
            // Arrange
            chargePoint.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 31, 23, 0, 0), DateTimeKind.Utc);
            chargePoint.Charge.Resolution = (int)Resolution.P1M;

            chargePoint2.Time = DateTime.SpecifyKind(new DateTime(2022, 2, 15, 23, 0, 0), DateTimeKind.Utc);
            chargePoint2.Charge.Resolution = (int)Resolution.P1M;

            chargePoint3.Time = DateTime.SpecifyKind(new DateTime(2022, 2, 20, 23, 0, 0), DateTimeKind.Utc);
            chargePoint3.Charge.Resolution = (int)Resolution.P1M;

            var expected = new List<ChargePriceV1Dto>
            {
                new(
                    chargePoint.Price,
                    chargePoint.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 2, 15, 23, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint2.Price,
                    chargePoint2.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 2, 20, 23, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint3.Price,
                    chargePoint3.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 2, 28, 23, 0, 0), DateTimeKind.Utc)),
            };

            var chargePoints = new List<ChargePoint> { chargePoint, chargePoint2, chargePoint3 }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.ToList().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsP1MWithMultiplePoints_ReturnsChargePointsWithCorrectDates(
            ChargePoint chargePoint,
            ChargePoint chargePoint2,
            ChargePoint chargePoint3)
        {
            // Arrange
            chargePoint.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 31, 23, 0, 0), DateTimeKind.Utc);
            chargePoint.Charge.Resolution = (int)Resolution.P1M;

            chargePoint2.Time = DateTime.SpecifyKind(new DateTime(2022, 2, 28, 23, 0, 0), DateTimeKind.Utc);
            chargePoint2.Charge.Resolution = (int)Resolution.P1M;

            chargePoint3.Time = DateTime.SpecifyKind(new DateTime(2022, 6, 30, 23, 0, 0), DateTimeKind.Utc);
            chargePoint3.Charge.Resolution = (int)Resolution.P1M;

            var expected = new List<ChargePriceV1Dto>
            {
                new(
                    chargePoint.Price,
                    chargePoint.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 2, 28, 23, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint2.Price,
                    chargePoint2.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 3, 31, 22, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint3.Price,
                    chargePoint3.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 7, 31, 22, 0, 0), DateTimeKind.Utc)),
            };

            var chargePoints = new List<ChargePoint> { chargePoint, chargePoint2, chargePoint3 }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.ToList().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsPT1H_TimeShouldNotOverlap(
            ChargePoint chargePoint,
            ChargePoint chargePoint2,
            ChargePoint chargePoint3)
        {
            // Arrange
            chargePoint.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 0, 0), DateTimeKind.Utc);
            chargePoint.Charge.Resolution = (int)Resolution.PT1H;

            chargePoint2.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 20, 0), DateTimeKind.Utc);
            chargePoint2.Charge.Resolution = (int)Resolution.PT1H;

            chargePoint3.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 2, 0, 0), DateTimeKind.Utc);
            chargePoint3.Charge.Resolution = (int)Resolution.PT1H;

            var expected = new List<ChargePriceV1Dto>
            {
                new(
                    chargePoint.Price,
                    chargePoint.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 1, 1, 20, 0), DateTimeKind.Utc)),
                new(
                    chargePoint2.Price,
                    chargePoint2.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 1, 2, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint3.Price,
                    chargePoint3.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 1, 3, 0, 0), DateTimeKind.Utc)),
            };

            var chargePoints = new List<ChargePoint> { chargePoint, chargePoint2, chargePoint3 }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.ToList().Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void AsChargePointV1Dto_WhenResolutionIsP1D_DatesShouldNotOverlap(
            ChargePoint chargePoint,
            ChargePoint chargePoint2,
            ChargePoint chargePoint3)
        {
            // Arrange
            chargePoint.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 1, 23, 0, 0), DateTimeKind.Utc);
            chargePoint.Charge.Resolution = (int)Resolution.P1D;

            chargePoint2.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 2, 14, 0, 0), DateTimeKind.Utc);
            chargePoint2.Charge.Resolution = (int)Resolution.P1D;

            chargePoint3.Time = DateTime.SpecifyKind(new DateTime(2022, 1, 2, 23, 0, 0), DateTimeKind.Utc);
            chargePoint3.Charge.Resolution = (int)Resolution.P1D;

            var expected = new List<ChargePriceV1Dto>
            {
                new(
                    chargePoint.Price,
                    chargePoint.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 2, 14, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint2.Price,
                    chargePoint2.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 2, 23, 0, 0), DateTimeKind.Utc)),
                new(
                    chargePoint3.Price,
                    chargePoint3.Time,
                    DateTime.SpecifyKind(new DateTime(2022, 1, 3, 23, 0, 0), DateTimeKind.Utc)),
            };

            var chargePoints = new List<ChargePoint> { chargePoint, chargePoint2, chargePoint3 }.AsQueryable();
            var iso8601Durations = GetIso8601Durations();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations);

            // Assert
            actual.ToList().Should().BeEquivalentTo(expected);
        }

        private static Iso8601Durations GetIso8601Durations()
        {
            var configuration = new Iso8601ConversionConfiguration("Europe/Copenhagen");
            var iso8601Durations = new Iso8601Durations(configuration);
            return iso8601Durations;
        }
    }
}
