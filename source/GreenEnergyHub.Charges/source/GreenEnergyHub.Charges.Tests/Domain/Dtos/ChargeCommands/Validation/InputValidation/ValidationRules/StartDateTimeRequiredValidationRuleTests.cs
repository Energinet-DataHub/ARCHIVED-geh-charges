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
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class StartDateTimeRequiredValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2020-05-10T13:00:00Z", true)]
        [InlineAutoMoqData("1970-01-01T00:00:00Z", false)]
        public void StartDateTimeRequiredValidationRule_NegativeTest(
            string startDateTime,
            bool expected,
            ChargeCommandBuilder chargeCommandBuilder)
        {
            var chargeCommand = CreateCommand(chargeCommandBuilder, startDateTime);
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new StartDateTimeRequiredValidationRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder chargeCommandBuilder)
        {
            var chargeCommand = CreateCommand(chargeCommandBuilder, "1970-01-01T00:00:00Z");
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new StartDateTimeRequiredValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.StartDateTimeRequiredValidation);
        }

        private static ChargeCommand CreateCommand(ChargeCommandBuilder builder, string startDateTime)
        {
            return builder.WithStartDateTime(InstantPattern.General.Parse(startDateTime).Value).Build();
        }
    }
}
