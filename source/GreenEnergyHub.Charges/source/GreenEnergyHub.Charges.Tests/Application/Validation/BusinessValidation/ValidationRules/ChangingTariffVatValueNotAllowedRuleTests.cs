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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.TestHelpers;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.BusinessValidation.ValidationRules
{
    public class ChangingTariffVatValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandDoesNotMatchCharge_IsFalse([NotNull]ChargeCommand command, [NotNull] Charge charge)
        {
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.False(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenVatPayerInCommandMatches_IsTrue([NotNull]ChargeCommand command, [NotNull] Charge charge)
        {
            command.MktActivityRecord.ChargeType.VatPayer = charge.MktActivityRecord.ChargeType.VatPayer;
            var sut = new ChangingTariffVatValueNotAllowedRule(command, charge);
            Assert.True(sut.IsValid);
        }
    }
}
