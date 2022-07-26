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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class MaximumPriceRuleTests
    {
        private const decimal LargestValidPrice = 999999;
        private const decimal SmallestInvalidPrice = 1000000;

        [Theory]
        [InlineAutoMoqData(999999, true)]
        [InlineAutoMoqData(999999.999999, true)]
        [InlineAutoMoqData(1000000, false)]
        [InlineAutoMoqData(1000000.000001, false)]
        public void MaximumPriceRule_WhenCalledPriceIsTooHigh_IsFalse(
            decimal price,
            bool expected,
            ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            var chargeOperationDto = CreateChargeOperationDto(chargePriceOperationDtoBuilder, price);
            var sut = new MaximumPriceRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            var chargePriceOperationDto = CreateChargeOperationDto(chargePriceOperationDtoBuilder, SmallestInvalidPrice);
            var sut = new MaximumPriceRule(chargePriceOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.MaximumPrice);
        }

        private static ChargePriceOperationDto CreateChargeOperationDto(ChargePriceOperationDtoBuilder builder, decimal price)
        {
            return builder.WithPoint(1, price).Build();
        }
    }
}
