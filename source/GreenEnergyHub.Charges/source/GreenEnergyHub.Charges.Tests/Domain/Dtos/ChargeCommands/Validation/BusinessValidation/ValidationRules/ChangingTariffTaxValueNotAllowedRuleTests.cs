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
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.Charges;
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
            ChargeOperationDtoBuilder builder,
            Charge charge)
        {
            var chargeOperationDto = builder.WithTaxIndicator(TaxIndicatorMapper.Map(!charge.TaxIndicator)).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenTaxIndicatorInCommandMatches_IsTrue(
            ChargeOperationDtoBuilder builder,
            Charge charge)
        {
            var chargeOperationDto = builder.WithTaxIndicator(TaxIndicatorMapper.Map(charge.TaxIndicator)).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder, Charge charge)
        {
            var chargeOperationDto = builder.WithTaxIndicator(TaxIndicatorMapper.Map(!charge.TaxIndicator)).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
        }

        [Theory]
        [InlineAutoDomainData]
        public void OperationId_ShouldBe_EqualTo(ChargeOperationDto chargeOperationDto, Charge charge)
        {
            var sut = new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge);
            sut.OperationId.Should().Be(chargeOperationDto.Id);
        }
    }
}
