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
    public class EnumComparisonTests
    {
        [Theory]
        [InlineData(typeof(ReferenceEnum), typeof(SubsetEnum), false)]
        [InlineData(typeof(ReferenceEnum), typeof(NothingSharedEnum), false)]
        [InlineData(typeof(ReferenceEnum), typeof(ExactlyLikeReferenceEnum), true)]
        [InlineData(typeof(ReferenceEnum), typeof(MatchingNameButMismatchingIntEnum), false)]
        [InlineData(typeof(ReferenceEnum), typeof(SubsetVariantEnum), false)]
        [InlineData(typeof(SubsetEnum), typeof(ReferenceEnum), true)]
        [InlineData(typeof(NothingSharedEnum), typeof(ReferenceEnum), false)]
        [InlineData(typeof(ExactlyLikeReferenceEnum), typeof(ReferenceEnum), true)]
        [InlineData(typeof(MatchingNameButMismatchingIntEnum), typeof(ReferenceEnum), false)]
        [InlineData(typeof(SubsetVariantEnum), typeof(ReferenceEnum), true)]
        public void IsSubsetOf_WhenExactStrategyIsUsed_ReturnsCorrectResult(
            Type subjectType,
            Type comparisonType,
            bool expected)
        {
            Assert.Equal(expected, EnumComparison.IsSubsetOf(subjectType, comparisonType, EnumComparisonStrategy.Exact));
        }

        private enum ReferenceEnum
        {
            A = 0,
            B = 1,
            C = 2,
        }

        private enum SubsetEnum
        {
            A = 0,
            B = 1,
        }

        private enum NothingSharedEnum
        {
            D = 0,
        }

        private enum ExactlyLikeReferenceEnum
        {
            A = 0,
            B = 1,
            C = 2,
        }

        private enum MatchingNameButMismatchingIntEnum
        {
            A = 0,
            C = 1,
        }

        private enum SubsetVariantEnum
        {
            A = 0,
            C = 2,
        }
    }
}
