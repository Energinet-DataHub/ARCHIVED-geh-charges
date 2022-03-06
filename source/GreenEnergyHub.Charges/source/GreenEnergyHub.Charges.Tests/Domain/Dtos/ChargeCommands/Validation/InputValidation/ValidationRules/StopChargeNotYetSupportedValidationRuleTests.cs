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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class StopChargeNotYetSupportedValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenNoEndDateTime_ReturnsTrue(ChargeCommandBuilder chargeCommandBuilder)
        {
            var chargeCommand = chargeCommandBuilder.WithEndDateTimeAsNull().Build();
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new StopChargeNotYetSupportedValidationRule(chargeOperationDto);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenEndDateTimeSet_ReturnsFalse(ChargeCommandBuilder chargeCommandBuilder)
        {
            var chargeCommand = chargeCommandBuilder.Build();
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new StopChargeNotYetSupportedValidationRule(chargeOperationDto);
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand chargeCommand)
        {
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new StopChargeNotYetSupportedValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.StopChargeNotYetSupported);
        }
    }
}
