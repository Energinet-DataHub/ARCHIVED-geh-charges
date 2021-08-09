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
using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.TestCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Core.Enumeration
{
    [UnitTest]
    public class ProtobufLenientValueNameComparisonStrategyTests
    {
        [Theory]
        [InlineAutoMoqData(ProtobufEnum.AA_SOMETHING, ComparisonEnum.Something, true)]
        [InlineAutoMoqData(ProtobufEnum.AA_SOMETHING, ComparisonEnum.EnergySupplier, false)]
        [InlineAutoMoqData(ProtobufEnum.MPR_ENERGY_SUPPLIER, ComparisonEnum.Something, false)]
        [InlineAutoMoqData(ProtobufEnum.MPR_ENERGY_SUPPLIER, ComparisonEnum.EnergySupplier, true)]
        public void IsEquivalent_WhenCompared_ReturnsCorrectResult(
            ProtobufEnum protobufValue,
            ComparisonEnum comparisonEnum,
            bool expected,
            [NotNull] ProtobufLenientValueNameComparisonStrategy sut)
        {
            var actual = sut.IsEquivalent(protobufValue, comparisonEnum);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsEquivalent_WhenProtobufEnumLackPrefix_ThrowsArgumentExceptions(
            [NotNull] ProtobufLenientValueNameComparisonStrategy sut)
        {
            Assert.Throws<ArgumentException>(() => sut.IsEquivalent(ProtobufEnum.NoPrefix, ComparisonEnum.Something));
        }

        public enum ProtobufEnum
        {
            AA_SOMETHING = 0,
            MPR_ENERGY_SUPPLIER = 1,
            NoPrefix = 2,
        }

        public enum ComparisonEnum
        {
            Something = 0,
            EnergySupplier = 1,
        }
    }
}
