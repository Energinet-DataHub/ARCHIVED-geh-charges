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
using GreenEnergyHub.Charges.Core.Enumeration;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Core.Enumeration
{
    [UnitTest]
    public class EnumValueNameComparisonStrategyFactoryTests
    {
        [Theory]
        [MemberData(nameof(AllStrategies))]
        public void Create_WhenCalled_ReturnsNotNullComparisonStrategy(EnumComparisonStrategy strategy)
        {
            var actual = EnumValueNameComparisonStrategyFactory.Create(strategy);
            Assert.NotNull(actual);
        }

        [Fact]
        public void Create_WhenCalledWithUnknownStrategy_ThrowsNotImplementedException()
        {
            var strategy = (EnumComparisonStrategy)987; // Some unused enum value
            Assert.Throws<NotImplementedException>(() => EnumValueNameComparisonStrategyFactory.Create(strategy));
        }

        public static IEnumerable<object[]> AllStrategies()
        {
            foreach (var strategy in Enum.GetValues(typeof(EnumComparisonStrategy)))
            {
                yield return new object[] { strategy! };
            }
        }
    }
}
