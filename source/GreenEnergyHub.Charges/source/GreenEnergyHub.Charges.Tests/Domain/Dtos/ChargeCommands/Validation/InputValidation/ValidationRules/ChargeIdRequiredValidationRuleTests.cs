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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeIdRequiredValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("ChargeId", true)]
        [InlineAutoMoqData("", false)]
        [InlineAutoMoqData(" ", false)]
        [InlineAutoMoqData(null!, false)]
        public void ChargeIdRequiredValidationRule_Test(
            string chargeId,
            bool expected,
            ChargeOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder.WithChargeId(chargeId).Build();
            var sut = new ChargeIdRequiredValidationRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder)
        {
            var invalidChargeOperationDto = builder.WithChargeId(string.Empty).Build();
            var sut = new ChargeIdRequiredValidationRule(invalidChargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeIdRequiredValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void OperationId_ShouldBe_EqualTo(ChargeOperationDto chargeOperationDto)
        {
            var sut = new ChargeIdRequiredValidationRule(chargeOperationDto);
            sut.OperationId.Should().Be(chargeOperationDto.Id);
        }
    }
}
