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
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Core.DateTime
{
    [UnitTest]
    public class TimestampExtensionsTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void ToInstant(DateTimeOffset expected)
        {
            var timestamp = Timestamp.FromDateTimeOffset(expected);
            var actual = timestamp.ToInstant();
            Assert.Equal(expected.UtcDateTime, actual.ToDateTimeOffset().UtcDateTime);
        }

        [Fact]
        public void TruncateToSeconds()
        {
            // Arrange
            var time = new System.DateTime(2021, 8, 2, 10, 58, 34, DateTimeKind.Utc);
            var expected = Timestamp.FromDateTime(time);
            var sut = Timestamp.FromDateTime(time.AddTicks(17));

            // Act
            var actual = sut.TruncateToSeconds();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
