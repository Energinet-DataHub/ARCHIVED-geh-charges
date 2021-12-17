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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class VatClassificationValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(VatClassification.NoVat, true)]
        [InlineAutoMoqData(VatClassification.Vat25, true)]
        [InlineAutoMoqData(VatClassification.Unknown, false)]
        public void VatClassificationValidationRule_Test(
            VatClassification vatClassification,
            bool expected,
            ChargeCommandTestBuilder chargeCommandTestBuilder)
        {
            var command = chargeCommandTestBuilder.WithVatClassification(vatClassification).Build();
            var sut = new VatClassificationValidationRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new VatClassificationValidationRule(command);
            sut.ValidationError.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.VatClassificationValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommand command)
        {
            // Arrange
            // Act
            var sut = new VatClassificationValidationRule(command);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters.Should()
                .Contain(ValidationErrorMessageParameterType.VatClass);
            sut.ValidationError.ValidationErrorMessageParameters.Should()
                .Contain(ValidationErrorMessageParameterType.PartyChargeTypeId);
        }
    }
}
