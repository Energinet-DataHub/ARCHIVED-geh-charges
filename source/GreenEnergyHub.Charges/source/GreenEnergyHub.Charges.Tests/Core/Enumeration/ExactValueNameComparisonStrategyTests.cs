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
using GreenEnergyHub.Charges.Core.Enumeration;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Core.Enumeration
{
    [UnitTest]
    public class ExactValueNameComparisonStrategyTests
    {
        [Theory]
        [InlineData(EnumOne.ANameOfTheValue, true)]
        [InlineData(EnumTwo.ANameOfTheValue, true)]
        [InlineData(EnumTwo.AnotherName, false)]
        [InlineData(EnumThree.ANAMEOFTHEVALUE, false)]
        public void IsEquivalent_WhenComparedToEnumOne_ReturnsCorrectResult(
            Enum value,
            bool expected)
        {
            var sut = new ExactValueNameComparisonStrategy();
            var actual = sut.IsEquivalent(EnumOne.ANameOfTheValue, value);
            Assert.Equal(expected, actual);
        }

        private enum EnumOne
        {
            ANameOfTheValue = 0,
        }

        private enum EnumTwo
        {
            ANameOfTheValue = 0,
            AnotherName = 1,
        }

        private enum EnumThree
        {
            ANAMEOFTHEVALUE = 0,
        }
    }
}
