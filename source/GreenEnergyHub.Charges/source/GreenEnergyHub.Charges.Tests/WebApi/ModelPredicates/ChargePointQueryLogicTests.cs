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
using Energinet.Charges.Contracts.ChargePoint;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.QueryApi.Model;
using GreenEnergyHub.Charges.QueryApi.ModelPredicates;
using GreenEnergyHub.Iso8601;
using Moq;
using NodaTime;
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
            ChargePoint chargePoint,
            Mock<IIso8601Durations> iso8601Durations)
        {
            // Arrange
            chargePoint.Time = new DateTime(2022, 1, 1, 1, 0, 0).ToUniversalTime();

            var expected = new ChargePointV1Dto(
                chargePoint.Price,
                chargePoint.Time,
                new DateTime(2022, 1, 1, 2, 15, 0).ToUniversalTime());

            iso8601Durations
                .Setup(i => i.GetTimeFixedToDuration(
                    It.IsAny<Instant>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(Instant.FromUtc(2022, 1, 1, 1, 15, 0));

            var chargePoints = new List<ChargePoint> { chargePoint }.AsQueryable();

            // Act
            var actual = chargePoints.AsChargePointV1Dto(iso8601Durations.Object);

            // Assert
            actual.Single().Should().Be(expected);
        }
    }
}
