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
    public class EnumComparisonTests
    {
        [Theory]
        [MemberData("AllStrategies")]
        public void IsSubsetOf_WhenComparingToSelf_IsAlwaysSubSet(EnumComparisonStrategy strategy)
        {
            Assert.True(EnumComparison.IsSubsetOf(typeof(EnumOne), typeof(EnumOne), strategy));
        }

        [Theory]
        [InlineData(typeof(EnumOne), typeof(EnumTwo), false)]
        [InlineData(typeof(EnumOne), typeof(EnumThree), false)]
        [InlineData(typeof(EnumOne), typeof(EnumFour), true)]
        [InlineData(typeof(EnumOne), typeof(EnumFive), false)]
        [InlineData(typeof(EnumOne), typeof(EnumSix), false)]
        [InlineData(typeof(EnumTwo), typeof(EnumOne), true)]
        [InlineData(typeof(EnumThree), typeof(EnumOne), false)]
        [InlineData(typeof(EnumFour), typeof(EnumOne), true)]
        [InlineData(typeof(EnumFive), typeof(EnumOne), false)]
        [InlineData(typeof(EnumSix), typeof(EnumOne), true)]
        public void IsSubsetOf_WhenExactStrategyIsUsed_ReturnsCorrectResult(
            Type subjectType,
            Type comparisonType,
            bool expected)
        {
            Assert.Equal(expected, EnumComparison.IsSubsetOf(subjectType, comparisonType, EnumComparisonStrategy.Exact));
        }

        private enum EnumOne
        {
            A = 0,
            B = 1,
            C = 2,
        }

        private enum EnumTwo
        {
            A = 0,
            B = 1,
        }

        private enum EnumThree
        {
            D = 0,
        }

        private enum EnumFour
        {
            A = 0,
            B = 1,
            C = 2,
        }

        private enum EnumFive
        {
            A = 0,
            C = 1,
        }

        private enum EnumSix
        {
            A = 0,
            C = 2,
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
