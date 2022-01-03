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

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeIdLengthValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("1234567891", true)]
        [InlineAutoMoqData("12345678912", false)]
        [InlineAutoMoqData(null!, false)]
        public void ChargeIdLengthValidationRule_Test(
            string chargeId,
            bool expected,
            ChargeCommandBuilder builder)
        {
            var command = builder.WithChargeId(chargeId).Build();
            var sut = new ChargeIdLengthValidationRule(command);
            Assert.Equal(expected, sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationError_WhenIsValid_IsNull(ChargeCommandBuilder builder)
        {
            var validCommand = CreateValidCommand(builder);
            var sut = new ChargeIdLengthValidationRule(validCommand);
            sut.ValidationError.Should().BeNull();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder)
        {
            var invalidCommand = CreateInvalidCommand(builder);
            var sut = new ChargeIdLengthValidationRule(invalidCommand);
            sut.ValidationError!.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeIdLengthValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommandBuilder builder)
        {
            var invalidCommand = CreateInvalidCommand(builder);
            var sut = new ChargeIdLengthValidationRule(invalidCommand);
            sut.ValidationError!.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(ChargeCommandBuilder builder)
        {
            var invalidCommand = CreateInvalidCommand(builder);
            var sut = new ChargeIdLengthValidationRule(invalidCommand);
            sut.ValidationError!.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .ParameterValue.Should().Be(invalidCommand.ChargeOperation.ChargeId);
        }

        private static ChargeCommand CreateValidCommand(ChargeCommandBuilder builder)
        {
            return builder.WithChargeId("ok").Build();
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder)
        {
            return builder.WithChargeId("this charge id is to long").Build();
        }
    }
}
