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

using GreenEnergyHub.Charges.Core.DateTime;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Core.DateTime
{
    [UnitTest]
    public class InstantExtensionsTests
    {
        [Fact]
        public void TimeOrEndDefault_WhenInstantIsNotNull_InReturnsInput()
        {
            // Arrange
            Instant? instant = SystemClock.Instance.GetCurrentInstant();

            // Act
            var result = instant.TimeOrEndDefault();

            // Assert
            Assert.Equal(instant!.Value, result);
        }

        [Fact]
        public void TimeOrEndDefault_WhenInstantIsNull_ReturnsDefaultEnd()
        {
            // Arrange
            Instant? instant = null;
            var expectedResult = Instant.FromUtc(9999, 12, 31, 23, 59, 59);

            // Act
            var result = instant.TimeOrEndDefault();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ToTimestamp_ReturnsInstant()
        {
            var instant = Instant.FromUtc(1992, 6, 26, 20, 15); // Start time of the match where Denmark won the European Football Championship
            var result = instant.ToTimestamp();
            Assert.Equal(instant.ToDateTimeOffset(), result.ToDateTimeOffset()); // Not a direct comparison, but the simplest way to assert the result
        }
    }
}
