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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChangingTariffVatValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandDoesNotMatchCharge_IsFalse([NotNull]ChargeCommand command, [NotNull] Charge charge)
        {
            command.ChargeOperation.VatClassification = (VatClassification)5;
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.False(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandMatches_IsTrue([NotNull]ChargeCommand command, [NotNull] Charge charge)
        {
            command.ChargeOperation.VatClassification = charge.VatClassification;
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.True(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo([NotNull]ChargeCommand command, [NotNull] Charge charge)
        {
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed);
        }
    }
}
