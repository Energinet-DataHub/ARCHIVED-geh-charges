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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChangingTariffTaxValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData(TaxIndicator.Tax, true, true)]
        [InlineAutoDomainData(TaxIndicator.Tax, false, false)]
        [InlineAutoDomainData(TaxIndicator.NoTax, false, true)]
        public void IsValid_WhenTaxIndicatorInCommandMatches_IsTrue(
            TaxIndicator newTaxIndicator,
            bool existingTaxIndicator,
            bool expected)
        {
            var sut = new ChangingTariffTaxValueNotAllowedRule(newTaxIndicator, existingTaxIndicator);
            sut.IsValid.Should().Be(expected);
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo()
        {
            var sut = new ChangingTariffTaxValueNotAllowedRule(
                It.IsAny<TaxIndicator>(),
                It.IsAny<bool>());
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
        }
    }
}
