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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class TransparentInvoicingMustBeFalseWhenCreatingFeeRuleTests
    {
        [Theory]
        [InlineAutoMoqData(ChargeType.Subscription, TransparentInvoicing.Transparent)]
        [InlineAutoMoqData(ChargeType.Subscription, TransparentInvoicing.NonTransparent)]
        [InlineAutoMoqData(ChargeType.Tariff, TransparentInvoicing.Transparent)]
        [InlineAutoMoqData(ChargeType.Tariff, TransparentInvoicing.NonTransparent)]
        public void IsValid_WhenChargeIsNotFee_ShouldReturnValid(ChargeType chargeType, TransparentInvoicing transparentInvoicing)
        {
            // Arrange
            var sut = new TransparentInvoicingMustBeFalseWhenCreatingFeeRule(chargeType, transparentInvoicing);

            // Act
            var isValid = sut.IsValid;

            // Assert
            isValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Fee, TransparentInvoicing.Transparent, false)]
        [InlineAutoMoqData(ChargeType.Fee, TransparentInvoicing.NonTransparent, true)]
        public void IsValid_WhenChargeIsFee_ShouldReturnExpectedResult(
            ChargeType chargeType,
            TransparentInvoicing transparentInvoicing,
            bool expectedResult)
        {
            // Arrange
            var sut = new TransparentInvoicingMustBeFalseWhenCreatingFeeRule(chargeType, transparentInvoicing);

            // Act
            var isValid = sut.IsValid;

            // Assert
            isValid.Should().Be(expectedResult);
        }
    }
}
