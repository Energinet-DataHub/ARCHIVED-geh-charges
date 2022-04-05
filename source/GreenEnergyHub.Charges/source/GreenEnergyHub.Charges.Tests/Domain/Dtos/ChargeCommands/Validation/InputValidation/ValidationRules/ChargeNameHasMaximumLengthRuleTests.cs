﻿// Copyright 2020 Energinet DataHub A/S
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

using System.Linq;
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
    public class ChargeNameHasMaximumLengthRuleTests
    {
        private const int ChargeNameMaximumLength = 50;

        [Theory]
        [InlineAutoMoqData(ChargeNameMaximumLength - 1, true)]
        [InlineAutoMoqData(ChargeNameMaximumLength, true)]
        [InlineAutoMoqData(ChargeNameMaximumLength + 1, false)]
        public void ChargeNameLengthValidationRule_WhenCalledWithChargeNameLength_EqualsExpectedResult(
            int chargeNameLength,
            bool expected,
            ChargeOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder.WithChargeName(new string('x', chargeNameLength)).Build();
            var sut = new ChargeNameHasMaximumLengthRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder
                .WithChargeName(new string('x', ChargeNameMaximumLength + 1))
                .Build();
            var sut = new ChargeNameHasMaximumLengthRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeNameHasMaximumLength);
        }
    }
}
