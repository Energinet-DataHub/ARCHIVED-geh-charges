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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChangingTariffVatValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandDoesNotMatchCharge_IsFalse(ChargeCommandBuilder builder, Charge charge)
        {
            var command = builder.WithVatClassification((VatClassification)5).Build();
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.False(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandMatches_IsTrue(ChargeCommandBuilder builder, Charge charge)
        {
            var command = builder.WithVatClassification(charge.VatClassification).Build();
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.True(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder, Charge charge)
        {
            var invalidCommand = CreateInvalidCommand(builder, charge);
            var sut = new ChangingTariffVatValueNotAllowedRule(invalidCommand, charge);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed);
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder, Charge charge)
        {
            // Get a new classification different from that of the charge
            var newClassification = charge.VatClassification == VatClassification.Vat25
                ? VatClassification.NoVat
                : VatClassification.Vat25;

            return builder.WithVatClassification(newClassification).Build();
        }
    }
}
