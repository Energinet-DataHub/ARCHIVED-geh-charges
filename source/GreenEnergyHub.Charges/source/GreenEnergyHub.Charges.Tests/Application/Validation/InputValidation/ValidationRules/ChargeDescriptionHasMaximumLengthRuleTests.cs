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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeDescriptionHasMaximumLengthRuleTests
    {
        private const int ChargeNameMaximumLength = 2048;

        [Theory]
        [InlineAutoMoqData(ChargeNameMaximumLength - 1, true)]
        [InlineAutoMoqData(ChargeNameMaximumLength, true)]
        [InlineAutoMoqData(ChargeNameMaximumLength + 1, false)]
        public void ChargeDescriptionHasMaximumLengthRule_WhenCalledWithChargeDescriptionLength_EqualsExpectedResult(
            int chargeDescriptionLength,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            command.ChargeOperation.ChargeName = GenerateStringWithLength(chargeDescriptionLength);
            var sut = new ChargeDescriptionHasMaximumLengthRule(command);
            sut.IsValid.Should().Be(expected);
        }

        private static string GenerateStringWithLength(int stringLength)
        {
            var repeatedChars = Enumerable.Repeat(0, stringLength).Select(n => "a");
            return string.Join(string.Empty, repeatedChars);
        }
    }
}
