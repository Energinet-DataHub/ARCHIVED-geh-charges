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

using System.Linq;
using FluentAssertions;
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
            ChargeCommandBuilder builder)
        {
            var command = builder.WithChargeName(GenerateStringWithLength(chargeNameLength)).Build();
            var sut = new ChargeNameHasMaximumLengthRule(command);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new ChargeNameHasMaximumLengthRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeNameHasMaximumLength);
        }

        private static string GenerateStringWithLength(int stringLength)
        {
            var repeatedChars = Enumerable.Repeat(0, stringLength).Select(_ => "a");
            return string.Join(string.Empty, repeatedChars);
        }
    }
}
