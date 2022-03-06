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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChangingTariffTaxValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenTaxIndicatorInCommandDoesNotMatchCharge_IsFalse(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            var invalidCommand = CreateInvalidCommand(builder, charge);
            var chargeOperationDto = invalidCommand.Charges.First();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            Assert.False(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenTaxIndicatorInCommandMatches_IsTrue(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            var chargeCommand = builder.WithTaxIndicator(charge.TaxIndicator).Build();
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            Assert.True(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder, Charge charge)
        {
            var invalidCommand = CreateInvalidCommand(builder, charge);
            var chargeOperationDto = invalidCommand.Charges.First();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder, Charge charge)
        {
            return builder.WithTaxIndicator(!charge.TaxIndicator).Build();
        }
    }
}
