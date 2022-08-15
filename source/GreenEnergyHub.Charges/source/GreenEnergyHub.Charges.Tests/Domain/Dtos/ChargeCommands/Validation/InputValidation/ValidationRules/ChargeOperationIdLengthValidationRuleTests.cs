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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.InputValidation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeOperationIdLengthValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("1_________10________20________123456", true)]
        [InlineAutoMoqData("1_________10________20________1234567", false)]
        public void OperationIdLengthValidationRule_Test(
            string operationId,
            bool expected,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            var chargeOperationDto = chargeInformationOperationDtoBuilder.WithChargeOperationId(operationId).Build();
            var sut = new ChargeOperationIdLengthValidationRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            var invalidChargeOperationDto = chargeInformationOperationDtoBuilder.Build();
            var sut = new ChargeOperationIdLengthValidationRule(invalidChargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeOperationIdLengthValidation);
        }

        [Fact]
        public void GivenChargeOperationIdLengthValidationRule_WhenOperationIdIsNull_ThenThrowException()
        {
            var chargeOperationDto = new ChargeInformationOperationDtoBuilder().WithChargeOperationId(null!).Build();
            var sut = new ChargeOperationIdLengthValidationRule(chargeOperationDto);
            Assert.Throws<NullReferenceException>(() => sut.IsValid);
        }
    }
}
